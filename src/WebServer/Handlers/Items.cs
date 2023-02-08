using Application.Items.Commands;
using Application.Items.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Items
{
    public static async Task<GetBasketVm> GetItems(IMediator m)
    {
        return await m.Send(new GetItemQuery());
    }

    public static async Task<long> AddItem(
        IMediator mediator,
        [FromBody] AddItemCommand cmd)
    {
        return await mediator.Send(cmd);
    }
}