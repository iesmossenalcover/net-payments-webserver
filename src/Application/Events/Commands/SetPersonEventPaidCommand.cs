using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Events.Commands;

public record SetPersonEventPaidCommand : EventData, IRequest<Response<bool>>
{
    private long _Id;

    public long GetId => _Id;
    public void SetId(long value) { _Id = value; }

    public bool Paid { get; set; } = false;
}

public class SetPersonEventPaidValidator : AbstractValidator<SetPersonEventPaidCommand>
{
    public SetPersonEventPaidValidator()
    {
        RuleFor(x => x.GetId).NotEmpty().WithMessage("S'ha l'ID");
    }
}

public class SetPersonEventPaidHandler : IRequestHandler<SetPersonEventPaidCommand, Response<bool>>
{
    #region IOC
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public SetPersonEventPaidHandler(IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }
    #endregion

    public async Task<Response<bool>> Handle(SetPersonEventPaidCommand request, CancellationToken ct)
    {
        var eventPerson = await _eventsPeopleRepository.GetByIdAsync(request.GetId, ct);
        if (eventPerson == null)
        {
            return Response<bool>.Error(ResponseCode.NotFound, "No s'ha trobat cap amb aquest id");
        }
        eventPerson.Paid = request.Paid;
        await _eventsPeopleRepository.UpdateAsync(eventPerson, CancellationToken.None);

        // TODO buissness logic: todo move
        // enrollment
        if (eventPerson.Event.Enrollment)
        {
            PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(eventPerson.PersonId, eventPerson.Event.CourseId, ct);
            if (pgc != null)
            {
                if (request.Paid)
                {
                    pgc.EnrollmentEvent = eventPerson.Event;
                    pgc.Enrolled = true;
                }
                else
                {
                    pgc.EnrollmentEvent = null;
                    pgc.Enrolled = false;
                }
                await _personGroupCourseRepository.UpdateAsync(pgc, CancellationToken.None);
            }
        }

        // amipa
        if (eventPerson.Event.Amipa)
        {
            PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(eventPerson.PersonId, eventPerson.Event.CourseId, ct);
            if (pgc != null)
            {
                pgc.Enrolled = request.Paid;
                await _personGroupCourseRepository.UpdateAsync(pgc, CancellationToken.None);
            }
        }
        return Response<bool>.Ok(true);
    }


}