using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Courses.Queries;

public record GetCourseQuery(long Id) : IRequest<Response<CourseVm>>;

public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, Response<CourseVm>>
{
    private readonly ICoursesRepository _coursesRepository;

    public GetCourseQueryHandler(ICoursesRepository coursesRepository)
    {
        _coursesRepository = coursesRepository;
    }

    public async Task<Response<CourseVm>> Handle(GetCourseQuery request, CancellationToken ct)
    {
        Course? course = await _coursesRepository.GetByIdAsync(request.Id, ct);
        if (course == null) return Response<CourseVm>.Error(ResponseCode.NotFound, "No s'ha trobat el curs que es vol editar");

        return Response<CourseVm>.Ok(new CourseVm(course.Id, course.Name, course.StartDate, course.EndDate, course.Active));
    }
}