using Application.Common;
using Application.Common.Models;
using Application.OURelations.Commands;
using Application.OURelations.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class OURelations
{
    // public static async Task<SelectorVm> GetOURelationsSelector(IMediator mediator)
    // {
    //     return await mediator.Send(new GetAllOURelationsSelectorQuery());
    // }

    public static async Task<IEnumerable<OURelationRowVm>> ListOURelations(
        IMediator mediator)
    {
        return await mediator.Send(new ListOURelationsQuery());
    }


    public static async Task<Response<OURelationVm>> GetOURelation(
        long id,
        IMediator mediator)
    {
        return await mediator.Send(new GetOURelationByIdQuery(id));
    }

    public static async Task<Response<long?>> CreateOURelation(
        IMediator mediator,
        [FromBody] CreateOURelationCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> UpdateOURelation(
    long id,
    IMediator mediator,
    [FromBody] UpdateOURelationCommand cmd)
    {
        cmd.Id = id;
        return await mediator.Send(cmd);
    }

        public static async Task<Response<long?>> DeleteOURelation(IMediator mediator, long id)
    {
        return await mediator.Send(new DeleteOURelationCommand(id));
    }


}