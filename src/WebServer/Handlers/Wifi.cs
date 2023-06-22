using Application.Common;
using Application.GoogleWorkspace.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Wifi
{
    public async static Task<Response<ExportWifiUsersVm>> ExportWifiUsers(IMediator m)
    {
        return await m.Send(new ExportWifiUsersCommand());
    }
}