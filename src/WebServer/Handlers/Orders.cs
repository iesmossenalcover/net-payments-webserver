using Application.Common;
using Application.Orders.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Orders
{
    public static async Task<Response<long?>> CreateOrder(IMediator mediator, [FromBody] CreateOrderCommand cmd)
    {
        return await mediator.Send(cmd);
    }
}