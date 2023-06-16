using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record SyncPeopleToGoogleWorkspaceCommand() : IRequest<Response<SyncPeopleToGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SyncPeopleToGoogleWorkspaceCommandVm();

// Handler
public class SyncPeopleToGoogleWorkspaceCommandHandler : IRequestHandler<SyncPeopleToGoogleWorkspaceCommand, Response<SyncPeopleToGoogleWorkspaceCommandVm>>
{
    #region IOC

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly ICoursesRepository _courseRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly string emailDomain;

    public SyncPeopleToGoogleWorkspaceCommandHandler(
        IOUGroupRelationsRepository oUGroupRelationsRepository,
        IGoogleAdminApi googleAdminApi,
        ICoursesRepository courseRepository,
        IPersonGroupCourseRepository personGroupCourseRepository,
        IPeopleRepository peopleRepository,
        ITasksRepository tasksRepository,
        IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _tasksRepository = tasksRepository;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
    }

    #endregion

    public async Task<Response<SyncPeopleToGoogleWorkspaceCommandVm>> Handle(SyncPeopleToGoogleWorkspaceCommand request, CancellationToken ct)
    {
        Domain.Entities.Tasks.Task task = new Domain.Entities.Tasks.Task()
        {
            Status = Domain.Entities.Tasks.TaskStatus.PENDING,
            Type = Domain.Entities.Tasks.TaskType.SYNC_USERS_TO_GOOGLE_WORKSPACE,
            Start = DateTimeOffset.UtcNow,
            End = null,
        };

        bool insertedTask = await _tasksRepository.AtomiInsertTaskAsync(task);
        if (!insertedTask)
        {
            return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja existeix una tasca del mateix tipus executant-se.");
        }

        return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Ok(new SyncPeopleToGoogleWorkspaceCommandVm());
    }
}