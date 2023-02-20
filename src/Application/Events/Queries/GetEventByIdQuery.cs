using Application.Common;
using Application.Common.Services;
using MediatR;

namespace Application.Events.Queries;

# region ViewModels
public record EventVm();

#endregion

#region Query
public record GetEventByIdQuery(long Id) : IRequest<Response<EventVm>>;
#endregion

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Response<EventVm>>
{
    #region  IOC
    private readonly ICoursesRepository _courseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public GetEventByIdQueryHandler(
        ICoursesRepository courseRepository,
        IPeopleRepository peopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _courseRepository = courseRepository;
        _peopleRepository = peopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }
    #endregion

    public Task<Response<EventVm>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        return null;
    }
}
