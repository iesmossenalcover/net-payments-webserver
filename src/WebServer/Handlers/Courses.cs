using Application.Common;
using Application.Common.Models;
using Application.Courses.Commands;
using Application.Courses.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers;

public class Courses
{
    public static async Task<Response<long>> CreateCourse(
        [FromBody] CreateCourseCommand cmd,
        IMediator mediator)
    {
        return await mediator.Send(cmd);
    }

    public static async Task<Response<Unit>> UpdateCourse(
        long id,
        [FromBody] UpdateCourseCommand cmd,
        IMediator mediator)
    {
        cmd.SetId(id);
        return await mediator.Send(cmd);
    }

    public static async Task<Response<Unit>> SetActiveCourse(
        long id,
        IMediator mediator)
    {
        var cmd = new SetActiveCourseCommand();
        cmd.SetId(id);
        return await mediator.Send(cmd);
    }

    public static async Task<Response<CourseVm>> GetCourse(IMediator mediator, long id)
    {
        return await mediator.Send(new GetCourseQuery(id));
    }

    public static async Task<IEnumerable<CourseVm>> GetAllCourses(IMediator mediator)
    {
        return await mediator.Send(new GetCoursesQuery());
    }

    public static async Task<SelectorVm> GetCoursesSelector(IMediator mediator)
    {
        return await mediator.Send(new GetAllCoursesSelectorQuery());
    }
}