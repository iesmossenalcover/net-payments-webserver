using Application.AdminInfo;
using Application.AdminInfo.Commands;
using Application.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class AdminInfo
{
    
  public static async Task<Response<long?>> UpdateAdminInfo(IMediator mediator, [FromBody] UpdateAppConfigCommand cmd)
    {
        return await mediator.Send(cmd);
    }

   public static async Task<AdminInfoVm> GetAdminInfo(IMediator mediator)
    {
        return await mediator.Send(new GetAdminInfoQuery());
    }


}
