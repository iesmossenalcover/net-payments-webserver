using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Courses.Commands;

public record CourseData
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public record CreateCourseCommand : CourseData, IRequest<Response<long>> { }

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    private readonly ICoursesRepository _courseRepository;

    public CreateCourseCommandValidator(ICoursesRepository courseRepository)
    {
        _courseRepository = courseRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("S'ha de proporcionar un nom.")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters.")
            .MustAsync(CheckUniqueNameAsync).WithMessage("Ja existeix un curs amb aquest nom.");

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("S'ha d'indicar una data d'inici de curs.");

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("S'ha d'indicar una data pel final de curs.")
            .GreaterThan(x => x.StartDate).WithMessage("La data de fi ha de ser posterior a la data d'inici");

    }

    private async Task<bool> CheckUniqueNameAsync(CreateCourseCommand cmd, string name, CancellationToken ct)
    {
        Course? c = await _courseRepository.GetCourseByNameAsync(name, ct);
        return c == null;
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
        if (!request.StartDate.HasValue || !request.EndDate.HasValue) return Response<long>.Error(ResponseCode.BadRequest, "S'han d'especificar una data d'inici i una data de fi");

        var course = new Course()
        {
            Name = request.Name,
            StartDate = request.StartDate.Value,
            EndDate = request.EndDate.Value,
            Active = false,
        };

        await _coursesRepository.InsertAsync(course, ct);

        return Response<long>.Ok(course.Id);
    }
}