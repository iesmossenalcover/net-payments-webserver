using Application.Items.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Items
{
    public static async Task<GetBasketVm> GetItems(IMediator m)
    {
        return await m.Send(new Application.Items.Queries.GetItemQuery());
    }

    public static async Task<long> AddItem(
        IMediator mediator,
        [FromBody] Application.Items.Commands.AddItemCommand cmd)
    {
        return await mediator.Send(cmd);
    }
}