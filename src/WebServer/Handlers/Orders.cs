using Application.Common;
using Application.Orders.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Orders
{
    public static async Task<Response<CreateOrderCommandVm?>> CreateOrder(IMediator mediator, [FromBody] CreateOrderCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<ConfirmOrderCommandVm?>> ConfirmOrder(
        IMediator mediator,
        [FromQuery(Name = "Ds_MerchantParameters")] string merchantParameters,
        [FromQuery(Name = "Ds_Signature")] string signature)
    {
        return await mediator.Send(new ConfirmOrderCommand() { MerchantParamenters = merchantParameters, Signature = signature });
    }

    public record RedsysResponse(string Ds_MerchantParameters, string Ds_Signature);
    public static async Task<Response<ConfirmOrderCommandVm?>> ConfirmOrderPost(
        IMediator mediator,
        [FromBody] RedsysResponse response)
    {
        return await mediator.Send(new ConfirmOrderCommand() { MerchantParamenters = response.Ds_MerchantParameters, Signature = response.Ds_Signature });
    }
}