using Application.Common;
using Application.Common.Services;
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
    public UpdateCourseCommandValidator()
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

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("S'ha d'indicar una data d'inici pel curs.")
            .GreaterThan(x => x.StartDate).WithMessage("La data de fi ha de ser posterior a la data d'inici");
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
        Course? course = await _coursesRepository.GetByIdAsync(request.GetId, ct);
        if (course == null) return Response<Unit>.Error(ResponseCode.NotFound, "El curs que es vol editar no existeix.");
        course.Name = request.Name;
        course.StartDate = request.StartDate;
        course.EndDate = request.EndDate;

        await _coursesRepository.UpdateAsync(course, ct);

        return Response<Unit>.Ok(Unit.Value);
    }
}