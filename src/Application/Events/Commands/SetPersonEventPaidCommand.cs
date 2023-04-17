using Application.Common;
using Application.Common.Services;
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

    public SetPersonEventPaidHandler(IEventsPeopleRespository eventsPeopleRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
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
        return Response<bool>.Ok(true);
    }


}