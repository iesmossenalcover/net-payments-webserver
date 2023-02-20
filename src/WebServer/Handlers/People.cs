using Application.Common;
using Application.People.Commands;
using Application.People.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class People
{
    public static async Task<ListPeopleByCourseVm> ListPeople(
        long? courseId,
        IMediator mediator)
    {
        return await mediator.Send(new ListPeopleByCourseQuery(courseId));
    }

    public static async Task<Response<PersonVm>> GetPerson(
        long id,
        IMediator mediator)
    {
        return await mediator.Send(new GetPersonByIdQuery(id));
    }

    public static async Task<Response<long?>> CreatePerson(
        IMediator mediator,
        [FromBody] CreatePersonCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<long?>> UpdatePerson(
        long id,
        IMediator mediator,
        [FromBody] UpdatePersonCommand cmd)
    {
        cmd.Id = id;
        return await mediator.Send(cmd);
    }

    public static async Task<long> DeletePerson(
        long id,
        IMediator mediator)
    {
        return await mediator.Send(new DeletePersonCommand(id));
    }
}