using Application.Common.Models;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Courses.Queries;

public record GetAllCoursesSelectorQuery : IRequest<SelectorVm>;

public class GetAllCoursesSelectorQueryHandler : IRequestHandler<GetAllCoursesSelectorQuery, SelectorVm>
{
    private readonly ICoursesRepository _coursesRepository;

    public GetAllCoursesSelectorQueryHandler(ICoursesRepository coursesRepository)
    {
        _coursesRepository = coursesRepository;
    }

    public async Task<SelectorVm> Handle(GetAllCoursesSelectorQuery request, CancellationToken ct)
    {
        IEnumerable<Course> courses = await _coursesRepository.GetAllAsync(true, ct);

        List<SelectOptionVm> options = new List<SelectOptionVm>(courses.Count());
        foreach (Course c in courses)
        {
            options.Add(new SelectOptionVm(c.Id.ToString(), c.Name));
        }

        return new SelectorVm(options);
    }
}