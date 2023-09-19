using Application.Common;
using Application.Processes.Commands;
using Application.Processes.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Processes
{
    public async static Task<GetLastProcessessQueryVm> GetProcessess(IMediator m)
    {
        return await m.Send(new GetLastProcessessQuery());
    }

    public async static Task<Response<StartProcessCommandVm>> StartProcess(IMediator m, [FromBody] StartProcessCommand cmd)
    {
        return await m.Send(cmd);
    }

    public async static Task<Response<GetLogQueryVm>> GetLog(IMediator m, long id)
    {
        return await m.Send(new GetLogQuery(id));
    }
}