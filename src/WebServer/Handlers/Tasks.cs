using Application.Common;
using Application.GoogleWorkspace.Commands;
using Application.Tasks.Commands;
using MediatR;

namespace WebServer.Handlers;

public class Tasks
{
    public static async Task<IResult> GetPeopleBatchUploadTemplate(IMediator mediator)
    {
        var response = await mediator.Send(new BatchUploadTemplateQuery());
        return Results.File(response.Stream.ToArray(), response.FileType, response.FileName);
    }

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
}