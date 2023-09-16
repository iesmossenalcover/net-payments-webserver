using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;
using Domain.Entities.Jobs;
using Domain.ValueObjects;
using Application.GoogleWorkspace.Commands.Processes;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record MovePeopleGoogleWorkspaceCommand() : IRequest<Response<MovePeopleGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record MovePeopleGoogleWorkspaceCommandVm(bool ok);

// TODO remove and use generic one

// Handler
public class MovePeopleGoogleWorkspaceCommandHandler : IRequestHandler<MovePeopleGoogleWorkspaceCommand, Response<MovePeopleGoogleWorkspaceCommandVm>>
{
    #region props
    private readonly IJobsRepository _jobsRepository;
    private readonly ProcessRunner _processRunner;
    private readonly string[] excludeEmails;

    public MovePeopleGoogleWorkspaceCommandHandler(IJobsRepository jobsRepository, IConfiguration configuration, ProcessRunner processRunner)
    {
        _jobsRepository = jobsRepository;
        _processRunner = processRunner;
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion

    public async Task<Response<MovePeopleGoogleWorkspaceCommandVm>> Handle(MovePeopleGoogleWorkspaceCommand request, CancellationToken ct)
    {
        // Queue the job if it is possible.
        var job = new Job()
        {
            Status = JobStatus.PENDING,
            Start = DateTimeOffset.UtcNow,
            Type = JobType.MOVE_PEOPLE_GOOGLE_WORKSPACE,
        };

        var queuedTask = await _jobsRepository.AtomicInsertJobAsync(job);
        if (!queuedTask)
        {
            return Response<MovePeopleGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus iniciada.");
        }

        var process = new MovePeopleGoogleWorkspaceProcess(excludeEmails);
        _processRunner.Start(process, job.Id);

        return Response<MovePeopleGoogleWorkspaceCommandVm>.Ok(new MovePeopleGoogleWorkspaceCommandVm(true));
    }
}