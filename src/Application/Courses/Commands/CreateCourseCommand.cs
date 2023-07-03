using Application.Common;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Courses.Commands;

public record CourseData
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

public record CreateCourseCommand : CourseData, IRequest<Response<long>> {}

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("S'ha de proporcionar un nom pel curs. Màxim 50 caràcters.");

        RuleFor(x => x.StartDate)
            .NotNull()
            .WithMessage("S'ha d'indicar una data d'inici pel curs.");

        RuleFor(x => x.EndDate)
            .NotNull()
            .WithMessage("S'ha d'indicar una data d'inici pel curs.");
    }
}

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Response<long>>
{

    #region IOC
    private readonly ICoursesRepository _coursesRepository;

    public CreateCourseCommandHandler(ICoursesRepository coursesRepository)
    {
        _coursesRepository = coursesRepository;
    }
    #endregion

    public async Task<Response<long>> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        var course = new Course()
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Active = false,
        };

        await _coursesRepository.InsertAsync(course, ct);

        return Response<long>.Ok(course.Id);
    }
}