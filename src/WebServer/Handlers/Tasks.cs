using Application.Tasks.Commands;
using MediatR;

namespace WebServer.Handlers;

public class Tasks
{
    public async static Task<IResult> UploadPeople(HttpContext ctx, IMediator m)
    {
        try
        {
            IFormFileCollection files = ctx.Request.Form.Files;
            if (files.Count() == 1)
            {
                IFormFile? f = files.First();
                if (f != null)
                {
                    Stream fileStream = f.OpenReadStream();
                    await m.Send(new PeopleBatchUploadCommand(fileStream));
                    fileStream.Close();
                }

                return Results.Ok();
            }
            return Results.BadRequest();
        }
        catch (System.Exception)
        {
            return Results.BadRequest();
        }
    }
}