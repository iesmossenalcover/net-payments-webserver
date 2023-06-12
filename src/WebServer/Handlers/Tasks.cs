using Application.Common;
using Application.Tasks.Commands;
using MediatR;

namespace WebServer.Handlers;

public class Tasks
{
    public async static Task<Response<BatchUploadSummary>> UploadPeople(HttpContext ctx, IMediator m)
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
        catch (Exception) {}

        if (f == null)
        {
            return Response<BatchUploadSummary>.Error(ResponseCode.BadRequest, "No s'ha pogut processar el fitxer.");
        }

        Stream fileStream = f.OpenReadStream();
        var result = await m.Send(new PeopleBatchUploadCommand(fileStream));
        fileStream.Close();
        return result;
    }

    public async static Task<Response<SyncStudentsCommandVm>> ProcessPeople(HttpContext ctx, IMediator m)
    {
        return await m.Send(new SyncStudentsCommand());
    }
}