using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using Infrastructure.Repos;
using MediatR;

namespace Application.Events.Queries;

# region ViewModels

#endregion

#region Query
public record ListEventsQuery(bool ShowExpired) : IRequest<IEnumerable<EventVm>>;
#endregion

public class ListEventsQueryHandler : IRequestHandler<ListEventsQuery, IEnumerable<EventVm>>
{
    #region  IOC
    private readonly IEventsRespository _eventsRepository;
    private readonly ICoursesRepository _coursesRepository;

    public ListEventsQueryHandler(IEventsRespository eventsRepository, ICoursesRepository coursesRepository)
    {
        _eventsRepository = eventsRepository;
        _coursesRepository = coursesRepository;
    }
    #endregion

    public async Task<IEnumerable<EventVm>> Handle(ListEventsQuery request, CancellationToken ct)
    {
        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);

        IEnumerable<Event> events = request.ShowExpired ? 
            await _eventsRepository.GetAllEventsByCourseIdAsync(course.Id, ct) :
            await _eventsRepository.GetAllUnexpiredEventsByCourseIdAsync(course.Id, ct);

        return events.Select(x => new EventVm(x.Id, x.Code, x.Description, x.Name, x.Price, x.AmipaPrice, x.Enrollment, x.Amipa, x.MaxQuantity, x.Date, x.CreationDate, x.PublishDate, x.UnpublishDate, x.IsActive));
    }
}
