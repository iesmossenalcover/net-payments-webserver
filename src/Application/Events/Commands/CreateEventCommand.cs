using Application.Common;
using Application.Common.Services;
using Application.People.Commands;
using Domain.Entities.Events;
using FluentValidation;
using MediatR;
using System.Text;
using System;

namespace Application.Events.Commands;

public record EventData
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal AmipaPrice { get; set; }
    public DateTimeOffset? PublishDate { get; set; } = default!;
    public DateTimeOffset? UnpublishDate { get; set; } = default!;
}

public record CreateEventCommand : EventData, IRequest<Response<long?>>
{ }

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
	public CreateEventCommandValidator()
	{
        RuleFor(x => x.Name).NotEmpty().WithMessage("S'ha de proporcionar un nom per l'event");
        RuleFor(x => x.Price).NotNull().GreaterThan(0).WithMessage("S'ha de posar un preu positiu");
        RuleFor(x => x.AmipaPrice).NotNull().GreaterThan(0).WithMessage("S'ha de posar un preu positiu");
        RuleFor(x => x.PublishDate).NotNull().WithMessage("S'ha de seleccionar una data de publicació");
        RuleFor(x => x.UnpublishDate)
            .Must((request, unpublish) => {
                if (!unpublish.HasValue) return true;

                if (unpublish.Value < request.PublishDate) return false;

                return true;
            }).WithMessage("La data ha de ser posterior a la data de publicació");
    }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Response<long?>>
{
    #region IOC
    private readonly int MAX_TRIES = 10;
    private readonly IEventsRespository _eventsRespository;

    public CreateEventCommandHandler(IEventsRespository eventsRespository)
    {
        _eventsRespository = eventsRespository;
    }
    #endregion

    public async Task<Response<long?>> Handle(CreateEventCommand request, CancellationToken ct)
    {
        bool foundFreeCode = false;
        string code = string.Empty;
        for (int i = 0; i < MAX_TRIES && !foundFreeCode; i++)
        {
            code = RandomString(5);
            Event? existingEvent = await _eventsRespository.GetEventByCodeAsync(code, ct);
            if (existingEvent == null) foundFreeCode = true;
        }

        if (!foundFreeCode)
        {
            throw new Exception("Can not found free code for the event");
        }

        Event e = new Event()
        {
            Code = code,
            Name = request.Name,
            CreationDate = DateTimeOffset.UtcNow,
            AmipaPrice = request.AmipaPrice,
            Price = request.Price,
            PublishDate = request.PublishDate ?? DateTimeOffset.UtcNow,
            UnpublishDate = request.UnpublishDate
        };

        await _eventsRespository.InsertAsync(e, CancellationToken.None);

        return Response<long?>.Ok(e.Id);
    }


    // move
    private static string RandomString(int length)
    {
        const string pool = "abcdefghijklmnopqrstuvwxyz";
        var builder = new StringBuilder(length);
        var random = new Random();
        for (var i = 0; i < length; i++)
        {
            var c = pool[random.Next(0, pool.Length)];
            builder.Append(c);
        }

        return builder.ToString();
    }
}