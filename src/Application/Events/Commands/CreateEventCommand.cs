using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using FluentValidation;
using MediatR;
using Domain.Entities.People;
using Application.Common.Helpers;

namespace Application.Events.Commands;

public record EventData
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal AmipaPrice { get; set; }
    public bool Enrollment { get; set; }
    public bool Amipa { get; set; }
    public uint MaxQuantity { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; } = default!;
    public DateTime PublishDate { get; set; }
    public DateTime? UnpublishDate { get; set; } = default!;
    public EventType? Type { get; set; }

    // Enrollment and AFA events are not outings, so they never require an authorization.
    public bool IsOuting => !Enrollment && !Amipa;
    public EventType ResolvedType => IsOuting ? Type ?? EventType.Other : EventType.Other;
    public string? ResolvedLocation => string.IsNullOrWhiteSpace(Location) ? null : Location.Trim();
}

public record CreateEventCommand : EventData, IRequest<Response<string?>>
{ }

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("S'ha de proporcionar un nom per l'esdeveniment");
        RuleFor(x => x.Price).NotNull().GreaterThanOrEqualTo(0).WithMessage("S'ha de posar un preu no negatiu");
        RuleFor(x => x.AmipaPrice).NotNull().GreaterThanOrEqualTo(0).WithMessage("S'ha de posar un preu no negatiu");
        RuleFor(x => x.Date).NotNull().WithMessage("S'ha de seleccionar una data.");
        RuleFor(x => x.EndDate)
            .Must((request, endDate) =>
            {
                if (!endDate.HasValue) return true;

                if (endDate.Value < request.Date) return false;

                return true;
            }).WithMessage("La data de finalització ha de ser posterior a la data d'inici");

        RuleFor(x => x.PublishDate).NotNull().WithMessage("S'ha de seleccionar una data de publicació");
        RuleFor(x => x.Type)
            .NotNull().WithMessage("S'ha d'indicar el tipus d'esdeveniment")
            .IsInEnum().WithMessage("Tipus d'esdeveniment no vàlid")
            .When(x => x.IsOuting);
        RuleFor(x => x.Location)
            .Must(x => Uri.TryCreate(x!.Trim(), UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("La ubicació ha de ser una URL vàlida")
            .When(x => !string.IsNullOrWhiteSpace(x.Location));
        RuleFor(x => x.MaxQuantity).Must(x => x > 0).WithMessage("La quanitat màxima ha de ser major o igual a 1.");
        RuleFor(x => x.UnpublishDate)
            .Must((request, unpublish) =>
            {
                if (!unpublish.HasValue) return true;

                if (unpublish.Value < request.PublishDate) return false;

                return true;
            }).WithMessage("La data ha de ser posterior a la data de publicació");
    }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Response<string?>>
{
    #region IOC
    private readonly int MAX_TRIES = 10;
    private readonly IEventsRespository _eventsRespository;
    private readonly ICoursesRepository _courseRepository;

    public CreateEventCommandHandler(IEventsRespository eventsRespository, ICoursesRepository courseRepository)
    {
        _eventsRespository = eventsRespository;
        _courseRepository = courseRepository;
    }
    #endregion

    public async Task<Response<string?>> Handle(CreateEventCommand request, CancellationToken ct)
    {
        bool foundFreeCode = false;
        string code = string.Empty;
        for (int i = 0; i < MAX_TRIES && !foundFreeCode; i++)
        {
            code = GenerateString.Random(5);
            Event? existingEvent = await _eventsRespository.GetEventByCodeAsync(code, ct);
            if (existingEvent == null) foundFreeCode = true;
        }

        if (!foundFreeCode)
        {
            throw new Exception("Can not found free code for the event");
        }

        Course course = await _courseRepository.GetCurrentCoursAsync(ct);

        Event e = new Event()
        {
            Code = code,
            Name = request.Name,
            CreationDate = DateTimeOffset.UtcNow,
            AmipaPrice = request.AmipaPrice,
            Enrollment = request.Enrollment,
            Amipa = request.Amipa,
            Type = request.ResolvedType,
            Price = request.Price,
            MaxQuantity = request.MaxQuantity,
            Description = request.Description,
            Location = request.ResolvedLocation,
            Date = new DateTimeOffset(request.Date.ToUniversalTime(), TimeSpan.Zero),
            EndDate = request.EndDate.HasValue ? new DateTimeOffset(request.EndDate.Value.ToUniversalTime(), TimeSpan.Zero) : null,
            PublishDate = new DateTimeOffset(request.PublishDate.ToUniversalTime(), TimeSpan.Zero),
            UnpublishDate = request.UnpublishDate.HasValue ? new DateTimeOffset(request.UnpublishDate.Value.ToUniversalTime(), TimeSpan.Zero) : null,
            Course = course
        };

        await _eventsRespository.InsertAsync(e, CancellationToken.None);

        return Response<string?>.Ok(e.Code);
    }
}