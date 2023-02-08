using Application.Tasks.Commands;
using MediatR;

namespace WebServer.Handlers;

public class Tasks
{
    public async static Task<IResult> UploadPeople(HttpContext ctx, IMediator m)
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
        catch (Exception)
        {
            return Results.BadRequest();
        }

        if (f == null)
        {
            return Results.BadRequest();
        }
        Stream fileStream = f.OpenReadStream();
        var result = await m.Send(new PeopleBatchUploadCommand(fileStream));
        fileStream.Close();
        return Results.Ok(result);
    }
}