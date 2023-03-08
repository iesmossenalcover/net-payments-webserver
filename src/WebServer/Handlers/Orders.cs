using Application.Common;
using Application.Orders.Commands;
using Application.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Orders
{
    public static async Task<Response<CreateOrderCommandVm?>> CreateOrder(IMediator mediator, [FromBody] CreateOrderCommand cmd)
    {
        return await mediator.Send(cmd);
    }
    
    public static async Task<Response<ConfirmOrderCommandVm?>> ConfirmOrderPost(
        IMediator mediator,
        HttpContext ctx)
    {
        return await mediator.Send(new ConfirmOrderCommand() {
            MerchantParamenters = ctx.Request.Form["Ds_MerchantParameters"].ToString(),
            Signature = ctx.Request.Form["Ds_Signature"].ToString()
        });
    }

    public static async Task<Response<OrderInfoVm>> GetOrderInfo(
        IMediator mediator,
        [FromQuery(Name = "Ds_MerchantParameters")] string merchantParameters,
        [FromQuery(Name = "Ds_Signature")] string signature,
        [FromQuery(Name = "Ds_SignatureVersion")] string signatureVersion)
    {
        return await mediator.Send(new OrderInfoQuery(signature, merchantParameters, signatureVersion));
    }
}
