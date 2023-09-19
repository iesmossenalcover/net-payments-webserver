using Domain.Entities.Jobs;
using Domain.Services;
using MediatR;

namespace Application.Processes.Queries;

# region ViewModels
public record JobVm(long Id, JobType Type, JobStatus Status, long? LogId, DateTimeOffset Start, DateTimeOffset? End);
public record GetLastProcessessQueryVm(IEnumerable<JobVm> Jobs);
#endregion

public record GetLastProcessessQuery() : IRequest<GetLastProcessessQueryVm>;

public class GetLastProcessessQueryHandler : IRequestHandler<GetLastProcessessQuery, GetLastProcessessQueryVm>
{
    # region IOC
    private readonly IJobsRepository _jobsRepository;

    public GetLastProcessessQueryHandler(IJobsRepository jobsRepository)
    {
        _jobsRepository = jobsRepository;
    }
    #endregion

    public async Task<GetLastProcessessQueryVm> Handle(GetLastProcessessQuery request, CancellationToken ct)
    {
        IEnumerable<Job> jobs = await _jobsRepository.GetLastOfEachType(ct);
        return new GetLastProcessessQueryVm(jobs.Select(x => new JobVm(x.Id, x.Type, x.Status, x.LogId, x.Start, x.End)));
    }
}
