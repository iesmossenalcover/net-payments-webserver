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
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;

    public SyncPeopleToGoogleWorkspaceCommandHandler(IGoogleAdminApi googleAdminApi)
    {
        _googleAdminApi = googleAdminApi;
    }

    public async Task<Response<SyncPeopleToGoogleWorkspaceCommandVm>> Handle(SyncPeopleToGoogleWorkspaceCommand request, CancellationToken ct)
    {
        // Domain.Entities.Tasks.Task task = new Domain.Entities.Tasks.Task()
        // {
        //     Status = Domain.Entities.Tasks.TaskStatus.PENDING,
        //     Type = Domain.Entities.Tasks.TaskType.SYNC_USERS_TO_GOOGLE_WORKSPACE,
        //     Start = DateTimeOffset.UtcNow,
        //     End = null,
        // };

        // bool insertedTask = await _tasksRepository.AtomiInsertTaskAsync(task);
        // if (!insertedTask)
        // {
        //     return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja existeix una tasca del mateix tipus executant-se.");
        // }

        // Process


        GoogleApiResult<bool> getUsersResult = await _googleAdminApi.DeleteUserInGroup("test1234@iesmossenalcover.cat", "payment.superuser@iesmossenalcover.cat");

        if (!getUsersResult.Success)
        {
            return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, getUsersResult.ErrorMessage ?? "");

        }
        return Response<SyncPeopleToGoogleWorkspaceCommandVm>.Ok(new SyncPeopleToGoogleWorkspaceCommandVm());
    }
    #endregion


}