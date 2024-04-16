using Application.Common;
using Application.GoogleWorkspace.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Wifi
{

    public static async Task<IResult> ExportWifiUsers(IMediator mediator)
    {
        var response = await mediator.Send(new ExportWifiUsersQuery());
        return Results.File(response.Stream.ToArray(), response.FileType, response.FileName);
    }
}