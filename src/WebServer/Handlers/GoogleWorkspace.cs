using Application.Common;
using Application.GoogleWorkspace.Commands;
using MediatR;

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
}