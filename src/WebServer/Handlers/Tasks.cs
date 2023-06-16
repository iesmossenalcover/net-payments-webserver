using Application.Common;
using Application.Tasks.Commands;
using MediatR;

namespace WebServer.Handlers;

public class Tasks
{
    public async static Task<Response<PeopleBatchUploadSummary>> PeopleBatchUpload(HttpContext ctx, IMediator m)
    {
        IFormFile? f = null;
        try
        {
            IFormFileCollection files = ctx.Request.Form.Files;
            if (files.Count == 1)
            {
                f = files[0];
            }
        }
        catch (Exception) { }

        if (f == null)
        {
            return Response<PeopleBatchUploadSummary>.Error(ResponseCode.BadRequest, "No s'ha pogut processar el fitxer.");
        }

        Stream fileStream = f.OpenReadStream();
        var result = await m.Send(new PeopleBatchUploadCommand(fileStream));
        fileStream.Close();
        return result;
    }

    public async static Task<Response<SyncPeopleToGoogleWorkspaceCommandVm>> SyncPeopleToGoogleWorkspace(IMediator m)
    {
        return await m.Send(new SyncPeopleToGoogleWorkspaceCommand());
    }

    public async static Task<Response<SyncPersonToGoogleWorkspaceCommandVm>> SyncPersonToGoogleWorkspace(long id, IMediator m)
    {
        return await m.Send(new SyncPersonToGoogleWorkspaceCommand(id));
    }
}