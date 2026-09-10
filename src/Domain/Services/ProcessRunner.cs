using Domain.Entities.Jobs;
using Domain.Entities.Logs;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Services;

public interface IProcess
{
    Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct);
}

public class ProcessRunner
{
    /*
        Marge de seguretat, no un límit previst: cap procés hi hauria d'arribar mai.
        Només serveix perquè un procés penjat no deixi el job en RUNNING per sempre,
        cosa que bloqueja llançar-ne un altre del mateix tipus (AtomicInsertJobAsync
        rebutja els jobs amb un PENDING o RUNNING del mateix tipus).
        Es pot ajustar amb la clau "ProcessTimeoutHours" de la configuració.
    */
    private const double DEFAULT_TIMEOUT_HOURS = 6;

    private readonly IServiceScopeFactory _serviceProvider;

    public ProcessRunner(IServiceScopeFactory serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Start(IProcess process, long jobId)
    {
        _ = Task.Run(async () =>
        {
            // IOC
            using var scope = _serviceProvider.CreateAsyncScope();
            ILogStore logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();
            IJobsRepository jobsRepository = scope.ServiceProvider.GetRequiredService<IJobsRepository>();
            IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            ILogger<ProcessRunner> logger = scope.ServiceProvider.GetRequiredService<ILogger<ProcessRunner>>();

            Job? job = await jobsRepository.GetByIdAsync(jobId, CancellationToken.None);
            if (job == null) return;

            job.Status = JobStatus.RUNNING;
            await jobsRepository.UpdateAsync(job, CancellationToken.None);

            Log log = new();
            log.Add("Starting process...");

            TimeSpan timeout = TimeSpan.FromHours(
                configuration.GetValue<double?>("ProcessTimeoutHours") ?? DEFAULT_TIMEOUT_HOURS);

            // No es posa dins d'un using: si s'esgota el marge la tasca segueix viva i
            // encara té el token; alliberar-lo aquí la podria fer petar.
            CancellationTokenSource cts = new(timeout);
            bool timedOut = false;

            try
            {
                Task runTask = process.Run(_serviceProvider, log, cts.Token);

                /*
                    Esperam la tasca o el marge, el que arribi primer. Cancel·lar no basta
                    per desencallar un procés penjat: cap crida a l'API de Google accepta
                    CancellationToken, així que si es penja dins d'una crida no se n'assabenta.
                    Per això, passat el marge, deixam d'esperar-la i tancam el job igualment.
                */
                Task completed = await Task.WhenAny(runTask, Task.Delay(timeout));

                if (completed == runTask)
                {
                    // Torna a llançar l'excepció de la tasca, si n'hi va haver.
                    await runTask;
                }
                else
                {
                    timedOut = true;
                    logger.LogError(
                        "El procés del job {JobId} ha superat el marge de {Hours} h i s'ha deixat d'esperar",
                        jobId, timeout.TotalHours);
                    log.Add($"[Avís] El procés ha superat el marge de {timeout.TotalHours} h i s'ha deixat d'esperar. Pot ser que hagi quedat a mitges: revisa el registre del sistema abans de tornar-lo a llançar.");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error executant el procés del job {JobId}", jobId);
                log.Add(e.Message);
            }
            finally
            {
                /*
                    Sense aquest finally, qualsevol excepció aquí baix (o un procés que no
                    acabava mai) deixava el job en RUNNING per sempre i impedia tornar a
                    llançar-ne un del mateix tipus.
                */
                // Si hem deixat d'esperar, la tasca segueix viva amb aquest token.
                if (!timedOut)
                {
                    cts.Dispose();
                }

                log.Add("Process finished");

                try
                {
                    LogStoreInfo logStoreInfo = await logStore.Save(log);

                    job.Log = logStoreInfo;
                    job.End = DateTimeOffset.UtcNow;
                    job.Status = JobStatus.FINISHED;

                    await jobsRepository.UpdateAsync(job, CancellationToken.None);
                }
                catch (Exception e)
                {
                    // Últim recurs: si ni tan sols podem tancar el job, que quedi constància
                    // al log del sistema, perquè el job es quedarà en RUNNING.
                    logger.LogError(e,
                        "No s'ha pogut tancar el job {JobId}; quedarà en estat RUNNING", jobId);
                }
            }
        });
    }
}
