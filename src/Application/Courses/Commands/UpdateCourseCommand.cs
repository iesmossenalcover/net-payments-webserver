using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Courses.Commands;

public record UpdateCourseCommand : CourseData, IRequest<Response<Unit>>
{
    private long _Id;

    public long GetId => _Id;
    public void SetId(long value) { _Id = value; }
}

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    private readonly ICoursesRepository _courseRepository;

    public UpdateCourseCommandValidator(ICoursesRepository courseRepository)
    {
        _courseRepository = courseRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("S'ha de proporcionar un nom pel curs.")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters.")
            .MustAsync(CheckUniqueNameAsync).WithMessage("Ja existeix un curs amb aquest nom.");

        RuleFor(x => x.StartDate)
            .NotNull()
            .WithMessage("S'ha d'indicar una data d'inici pel curs.");

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("S'ha d'indicar una data de fi de curs.")
            .GreaterThan(x => x.StartDate).WithMessage("La data de fi ha de ser posterior a la data d'inici");
    }

    private async Task<bool> CheckUniqueNameAsync(UpdateCourseCommand cmd, string name, CancellationToken ct)
    {
        Course? c = await _courseRepository.GetCourseByNameAsync(name, ct);
        if (c == null)
        {
            return true;
        }
        else
        {
            return c.Id == cmd.GetId;
        }
    }
}

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Response<Unit>>
{

    #region IOC
    private readonly ICoursesRepository _coursesRepository;

    public UpdateCourseCommandHandler(ICoursesRepository coursesRepository)
    {
        _coursesRepository = coursesRepository;
    }
    #endregion

    public async Task<Response<Unit>> Handle(UpdateCourseCommand request, CancellationToken ct)
    {
        if (!request.StartDate.HasValue || !request.EndDate.HasValue) return Response<Unit>.Error(ResponseCode.BadRequest, "S'han d'especificar una data d'inici i una data de fi");

        Course? course = await _coursesRepository.GetByIdAsync(request.GetId, ct);
        if (course == null) return Response<Unit>.Error(ResponseCode.NotFound, "El curs que es vol editar no existeix.");
        course.Name = request.Name;
        course.StartDate = request.StartDate.Value;
        course.EndDate = request.EndDate.Value;

        await _coursesRepository.UpdateAsync(course, ct);

        return Response<Unit>.Ok(Unit.Value);
    }
}