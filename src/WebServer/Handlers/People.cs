
using Application.People.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class People
{
    public static async Task<long> CreatePerson(
        IMediator mediator,
        [FromBody] CreatePersonCommand cmd)
    {
        return await mediator.Send(cmd);
    }
}