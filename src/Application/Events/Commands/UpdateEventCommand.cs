using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using FluentValidation;
using MediatR;

namespace Application.Events.Commands;

public record UpdateEventCommand : EventData, IRequest<Response<long?>>
{
    private long _Id;

    public long GetId => _Id;
    public void SetId(long value) { _Id = value; }
}

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("S'ha de proporcionar un nom per l'event");
        RuleFor(x => x.Price).NotNull().GreaterThan(0).WithMessage("S'ha de posar un preu positiu");
        RuleFor(x => x.AmipaPrice).NotNull().GreaterThan(0).WithMessage("S'ha de posar un preu positiu");
        RuleFor(x => x.PublishDate).NotNull().WithMessage("S'ha de seleccionar una data de publicació");
        RuleFor(x => x.UnpublishDate)
            .Must((request, unpublish) =>
            {
                if (!unpublish.HasValue) return true;

                if (unpublish.Value < request.PublishDate) return false;

                return true;
            }).WithMessage("La data ha de ser posterior a la data de publicaci�");
    }
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Response<long?>>
{
    #region IOC
    private readonly IEventsRespository _eventsRespository;

    public UpdateEventCommandHandler(IEventsRespository eventsRespository)
    {
        _eventsRespository = eventsRespository;
    }
    #endregion

    public async Task<Response<long?>> Handle(UpdateEventCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRespository.GetByIdAsync(request.GetId, ct);
        if (e == null) return Response<long?>.Error(ResponseCode.NotFound, "L'event que es vol modificar no existeix.");

        e.Name = request.Name;
        e.AmipaPrice = request.AmipaPrice;
        e.Price = request.Price;
        e.PublishDate = new DateTimeOffset(request.PublishDate.ToUniversalTime(), TimeSpan.Zero);
        e.UnpublishDate =  request.UnpublishDate.HasValue ? new DateTimeOffset(request.UnpublishDate.Value.ToUniversalTime(), TimeSpan.Zero) : null;
        e.Enrollment = request.Enrollment;
        e.Amipa = request.Amipa;
        e.Description = request.Description;

        await _eventsRespository.UpdateAsync(e, CancellationToken.None);

        return Response<long?>.Ok(e.Id);
    }


}