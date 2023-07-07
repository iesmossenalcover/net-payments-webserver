using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using FluentValidation;
using MediatR;
using Domain.Behaviours;

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
    private readonly ICoursesRepository _coursesRepository;
    private readonly Domain.Behaviours.EventPersonProcessingService _eventPersonProcessingService;

    public SetPersonEventPaidHandler(IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository personGroupCourseRepository, ICoursesRepository coursesRepository, EventPersonProcessingService eventPersonProcessingService)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _coursesRepository = coursesRepository;
        _eventPersonProcessingService = eventPersonProcessingService;
    }

    #endregion

    public async Task<Response<bool>> Handle(SetPersonEventPaidCommand request, CancellationToken ct)
    {
        EventPerson? eventPerson = await _eventsPeopleRepository.GetWithRelationsByIdAsync(request.GetId, ct);
        if (eventPerson == null) return Response<bool>.Error(ResponseCode.NotFound, "Aquesta persona no està  a l'esdeveniment.");
        
        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        if (course.Id != eventPerson.Event.CourseId) return Response<bool>.Error(ResponseCode.BadRequest, "No es pot fer un pagament d'un curs no actiu.");

        PersonGroupCourse? pgc = await _personGroupCourseRepository.GetCoursePersonGroupById(eventPerson.PersonId, course.Id, ct);
        if (pgc == null) return Response<bool>.Error(ResponseCode.BadRequest, "Error, la persona no està asociada al curs");

        eventPerson.Paid = request.Paid;
        eventPerson.PaidAsAmipa = request.Paid ? pgc.Amipa : false;
        
        await _eventsPeopleRepository.UpdateAsync(eventPerson, CancellationToken.None);

        // Bussiness logic when an event is paid/unpaid
        await _eventPersonProcessingService.ProcessPaidEvent(eventPerson, request.Paid, CancellationToken.None);

        return Response<bool>.Ok(true);
    }
}