using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Courses.Queries;

public record CourseVm(long Id, string Name, DateTimeOffset StartDate, DateTimeOffset EndDate, bool Active);
public record GetCoursesQuery() : IRequest<IEnumerable<CourseVm>>;

public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, IEnumerable<CourseVm>>
{
    private readonly ICoursesRepository _coursesRepository;

    public GetCoursesQueryHandler(ICoursesRepository coursesRepository)
    {
        _coursesRepository = coursesRepository;
    }

    public async Task<IEnumerable<CourseVm>> Handle(GetCoursesQuery request, CancellationToken ct)
    {
        IEnumerable<Course> courses = await _coursesRepository.GetAllAsync(ct);
        return courses.Select(x => new CourseVm(x.Id, x.Name, x.StartDate, x.EndDate, x.Active));
    }
}