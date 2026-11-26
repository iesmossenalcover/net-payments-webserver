using Application.Common;
using Domain.Entities.EventRequests;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Queries;

/// <summary>
/// Detall d'una sol·licitud. Amb OnlyMine, només si és de l'usuari actual (vista del professor);
/// sense, qualsevol (vista de l'admin).
/// </summary>
public record GetEventRequestQuery(long Id, bool OnlyMine) : IRequest<Response<EventRequestVm>>;

public class GetEventRequestQueryHandler : IRequestHandler<GetEventRequestQuery, Response<EventRequestVm>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IGroupsRepository _groupsRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly ICurrentRequestService _currentRequestService;

    public GetEventRequestQueryHandler(IEventRequestsRepository eventRequestsRepository, IPeopleRepository peopleRepository, IGroupsRepository groupsRepository, IPersonGroupCourseRepository personGroupCourseRepository, ICurrentRequestService currentRequestService)
    {
        _personGroupCourseRepository = personGroupCourseRepository;
        _eventRequestsRepository = eventRequestsRepository;
        _peopleRepository = peopleRepository;
        _groupsRepository = groupsRepository;
        _currentRequestService = currentRequestService;
    }
    #endregion

    public async Task<Response<EventRequestVm>> Handle(GetEventRequestQuery request, CancellationToken ct)
    {
        EventRequest? r = await _eventRequestsRepository.GetWithDetailsAsync(request.Id, ct);
        if (r == null || (request.OnlyMine && (_currentRequestService.UserId is not long userId || !r.IsOwnedBy(userId))))
        {
            return Response<EventRequestVm>.Error(ResponseCode.NotFound, EventRequestActions.NOT_FOUND_MESSAGE);
        }

        return Response<EventRequestVm>.Ok(await EventRequestMapper.ToVmAsync(r, _peopleRepository, _groupsRepository, _personGroupCourseRepository, ct));
    }
}
