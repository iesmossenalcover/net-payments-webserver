using Application.Common;
using Application.GoogleWorkspace.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class GoogleWorkspace
{

    public async static Task<Response<SyncPeopleToGoogleWorkspaceCommandVm>> SyncPeopleToGoogleWorkspace(IMediator m)
    {
        return await m.Send(new SyncPeopleToGoogleWorkspaceCommand());
    }

    public async static Task<Response<SyncPersonToGoogleWorkspaceCommandVm>> SyncPersonToGoogleWorkspace(long id, IMediator m)
    {
        return await m.Send(new SyncPersonToGoogleWorkspaceCommand(id));
    }

    public async static Task<Response<SetPasswordGoogleWorkspaceCommandVm>> UpdatePasswordGoogleWorkspace(long id, IMediator m)
    {
        return await m.Send(new SetPasswordGoogleWorkspaceCommand(id));
    }

    public async static Task<Response<SuspendGoogleWorkspaceCommandVm>> SuspendPeopleByOuGoogleWorkspace(IMediator m)
    {
        return await m.Send(new SuspendGoogleWorkspaceCommand());
    }

    public async static Task<Response<MovePeopleGoogleWorkspaceCommandVm>> MovePeopleByOuGoogleWorkspace(IMediator m)
    {
        return await m.Send(new MovePeopleGoogleWorkspaceCommand());
    }
    public async static Task<Response<AddPeopleToGroupGoogleWorkspaceCommandVm>> AddPeopleToGroupGoogleWorkspace(IMediator m)
    {
        return await m.Send(new AddPeopleToGroupGoogleWorkspaceCommand());
    }


    public static async Task<IResult> ExportPeopleGoogleWorkspace(IMediator mediator)
    {
        var response = await mediator.Send(new ExportSyncPeopleGoogleWorkspaceCommand());
        byte[] byteArray = response.Stream.ToArray();
        return Results.File(byteArray, response.FileType, response.FileName);
    }
}