using Application.Common;
using Domain.Entities.Events;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

# region ViewModels

public record PersonPaymentInfoVm(long EventPersonId, long EventId, string EventName, decimal Amount,
    bool ManualPayment, DateTimeOffset? PaidDate);

public record PersonCoursePaymentsVm(long CourseId, string CourseName, IEnumerable<PersonPaymentInfoVm> Payments);

public record GetPersonPaymentsVm(IEnumerable<PersonCoursePaymentsVm> CoursePayments);

#endregion

public record GetPersonPaymentsQuery(long Id) : IRequest<Response<GetPersonPaymentsVm>>;

public class GetPersonPaymentsQueryHandler : IRequestHandler<GetPersonPaymentsQuery, Response<GetPersonPaymentsVm>>
{
    # region IOC

    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPeopleRepository _peopleRepository;

    public GetPersonPaymentsQueryHandler(IEventsPeopleRespository eventsPeopleRepository,
        IPeopleRepository peopleRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _peopleRepository = peopleRepository;
    }

    #endregion

    public async Task<Response<GetPersonPaymentsVm>> Handle(GetPersonPaymentsQuery request, CancellationToken ct)
    {
        Person? person = await _peopleRepository.GetByIdAsync(request.Id, ct);
        if (person == null)
            return Response<GetPersonPaymentsVm>.Error(ResponseCode.NotFound,
                @"No s'ha trobat cap persona amb aquest id");

        IEnumerable<EventPerson> events = await _eventsPeopleRepository.GetAllByPersonId(person.Id, ct);
        var groupedEvents = events.GroupBy(x => x.Event.Course).OrderByDescending(x => x.Key.StartDate);
        var coursesVm = groupedEvents.Select(x => new PersonCoursePaymentsVm(
            x.First().Event.CourseId,
            x.First().Event.Course.Name,
            x
                .OrderByDescending(y => y.DatePaid)
                .Select(y => new PersonPaymentInfoVm(
                    y.Id,
                    y.EventId,
                    y.Event.Name,
                    y.AmmountPaid(y.Event), y.OrderId.HasValue, y.DatePaid))
        ));
        return Response<GetPersonPaymentsVm>.Ok(new GetPersonPaymentsVm(coursesVm));
    }
}