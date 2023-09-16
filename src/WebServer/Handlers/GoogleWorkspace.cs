using Application.Common;
using Application.GoogleWorkspace.Commands;
using Application.Processes.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class GoogleWorkspace
{

    public async static Task<Response<SyncPersonToGoogleWorkspaceCommandVm>> SyncPersonToGoogleWorkspace(long id, IMediator m)
    {
        return await m.Send(new SyncPersonToGoogleWorkspaceCommand(id));
    }

    public async static Task<Response<SetPasswordGoogleWorkspaceCommandVm>> UpdatePasswordGoogleWorkspace(long id, IMediator m)
    {
        return await m.Send(new SetPasswordGoogleWorkspaceCommand(id));
    }

    public async static Task<Response<UpdateUserOUAndGroupWorkspaceCommandVm>> MoveOUGoogleWorkspace(long id, IMediator m)
    {
        return await m.Send(new UpdateUserOUAndGroupWorkspaceCommand(id));
    }

    public async static Task<Response<StartProcessCommandVm>> StartProcess(IMediator m, [FromBody] StartProcessCommand cmd)
    {
        return await m.Send(cmd);
    }

    public static async Task<IResult> ExportPeopleGoogleWorkspace(IMediator mediator)
    {
        var response = await mediator.Send(new ExportSyncPeopleGoogleWorkspaceCommand());
        return Results.File(response.Stream.ToArray(), response.FileType, response.FileName);
    }
}