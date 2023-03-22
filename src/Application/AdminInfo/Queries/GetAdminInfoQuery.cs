using Application.Common;
using Application.Common.Services;
using Domain.Entities.Configuration;
using Domain.Entities.Events;
using Domain.Entities.Orders;
using Domain.Entities.People;
using MediatR;

namespace Application.AdminInfo;

# region ViewModels

public record AppConfigVm(
    bool DisplayEnrollment
);

public record AdminInfoVm(
    int Events,
    int ActiveEvents,
    int EventsEndToday,
    int Grups,
    int People,
    int TodayPayments,
    AppConfigVm AppConfig
);

#endregion

#region Query
public record GetAdminInfoQuery() : IRequest<AdminInfoVm>;
#endregion

public class GetAdminInfoQueryHandler : IRequestHandler<GetAdminInfoQuery, AdminInfoVm>
{
    #region  IOC
    private readonly ICoursesRepository _coursesRepository;
    private readonly IEventsRespository _eventsRepository;
    private readonly IGroupsRepository _groupsRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IOrdersRepository _ordersReposiroty;
    private readonly IAppConfigRepository _appConfigReposiroty;

    public GetAdminInfoQueryHandler(ICoursesRepository coursesRepository, IEventsRespository eventsRepository, IGroupsRepository groupsRepository, IPersonGroupCourseRepository personGroupCourseRepository, IOrdersRepository ordersReposiroty, IAppConfigRepository appConfigReposiroty)
    {
        _coursesRepository = coursesRepository;
        _eventsRepository = eventsRepository;
        _groupsRepository = groupsRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _ordersReposiroty = ordersReposiroty;
        _appConfigReposiroty = appConfigReposiroty;
    }


    #endregion
    public async Task<AdminInfoVm> Handle(GetAdminInfoQuery request, CancellationToken ct)
    {
        Course c = await _coursesRepository.GetCurrentCoursAsync(ct);
        IEnumerable<Event> events = await _eventsRepository.GetAllEventsByCourseIdAsync(c.Id, ct);
        IEnumerable<Group> groups = await _groupsRepository.GetAllAsync(ct);
        IEnumerable<Order> orders = await _ordersReposiroty.GetTodayPaidOrdersAsync(ct);
        IEnumerable<PersonGroupCourse> pgcs = _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(c.Id,ct).ToList();
        AppConfig appConfig = await _appConfigReposiroty.GetAsync(ct);

        return new AdminInfoVm(
            events.Count(),
            events.Count(x => x.IsActive),
            events.Count(x => x.UnpublishDate == DateTimeOffset.Now),
            groups.Count(),
            pgcs.Count(),
            orders.Count(),
            new AppConfigVm(appConfig.DisplayEnrollment)
        );


    }
}
