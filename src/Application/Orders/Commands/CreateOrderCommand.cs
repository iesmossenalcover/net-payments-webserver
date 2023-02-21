using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using FluentValidation;
using MediatR;
using System.Text;
using Domain.Entities.People;

namespace Application.Orders.Commands;

public record CreateOrderCommand : IRequest<Response<long?>>
{
    public string DocumentId { get; set; } = string.Empty;
    public IEnumerable<string> EventCodes { get; set; } = Enumerable.Empty<string>();
}

public class CreateEventCommandValidator : AbstractValidator<CreateOrderCommand>
{
	public CreateEventCommandValidator()
	{
        RuleFor(x => x.EventCodes).NotEmpty().WithMessage("Com a mínim s'ha de seleccionar un event.");
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Response<long?>>
{
    #region IOC
    private readonly IEventsRespository _eventsRespository;
    private readonly ICoursesRepository _courseRepository;

    #endregion

    public async Task<Response<long?>> Handle(CreateOrderCommand request, CancellationToken ct)
    {

        return Response<long?>.Ok(1);
    }
}