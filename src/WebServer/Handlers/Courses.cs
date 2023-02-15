using Application.Common.Models;
using Application.Courses.Queries;
using Application.People.Queries;
using MediatR;

namespace WebServer.Handlers;

public class Courses
{
    public static async Task<SelectorVm> GetCoursesSelector(IMediator mediator)
    {
        return await mediator.Send(new GetAllCoursesSelectorQuery());
    }
}