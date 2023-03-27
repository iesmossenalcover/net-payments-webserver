using Application.Common;
using Application.Common.Services;
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
    public string Description { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public DateTime? UnpublishDate { get; set; } = default!;
}

public record CreateEventCommand : EventData, IRequest<Response<string?>>
{ }

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("S'ha de proporcionar un nom per l'event");
        RuleFor(x => x.Price).NotNull().GreaterThan(0).WithMessage("S'ha de posar un preu positiu");
        RuleFor(x => x.AmipaPrice).NotNull().GreaterThan(0).WithMessage("S'ha de posar un preu positiu");
        RuleFor(x => x.PublishDate).NotNull().WithMessage("S'ha de seleccionar una data de publicaci�");
        RuleFor(x => x.UnpublishDate)
            .Must((request, unpublish) =>
            {
                if (!unpublish.HasValue) return true;

                if (unpublish.Value < request.PublishDate) return false;

                return true;
            }).WithMessage("La data ha de ser posterior a la data de publicaci�");
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
            Price = request.Price,
            Description = request.Description,
            PublishDate = new DateTimeOffset(request.PublishDate.ToUniversalTime(), TimeSpan.Zero),
            UnpublishDate = request.UnpublishDate.HasValue ? new DateTimeOffset(request.UnpublishDate.Value.ToUniversalTime(), TimeSpan.Zero) : null,
            Course = course
        };

        await _eventsRespository.InsertAsync(e, CancellationToken.None);

        return Response<string?>.Ok(e.Code);
    }
}