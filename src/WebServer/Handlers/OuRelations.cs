using Application.Common;
using Application.Common.Models;
using Application.OuRelations.Commands;
using Application.OuRelations.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class OuRelations
{
    public static async Task<IEnumerable<OuRelationRowVm>> ListOuRelations(
        IMediator mediator)
    {
        return await mediator.Send(new ListOuRelationsQuery());
    }


    public static async Task<Response<OuRelationVm>> GetOuRelation(
        long id,
        IMediator mediator)
    {
        return await mediator.Send(new GetOuRelationByIdQuery(id));
    }

    public static async Task<Response<long?>> CreateOuRelation(
        IMediator mediator,
        [FromBody] CreateOuRelationCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> UpdateOuRelation(
    long id,
    IMediator mediator,
    [FromBody] UpdateOuRelationCommand cmd)
    {
        cmd.Id = id;
        return await mediator.Send(cmd);
    }

        public static async Task<Response<long?>> DeleteOuRelation(IMediator mediator, long id)
    {
        return await mediator.Send(new DeleteOuRelationCommand(id));
    }


}