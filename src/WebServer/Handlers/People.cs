
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

    public static async Task<long> CreatePerson(
        IMediator mediator,
        [FromBody] CreatePersonCommand cmd)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<long> UpdatePerson(
        IMediator mediator,
        [FromBody] UpdatePersonCommand cmd)
    {
        return await mediator.Send(cmd);
    }
}