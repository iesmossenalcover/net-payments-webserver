using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.People.Commands;

// Model we receive
public record UpdatePersonCommand : CreatePersonCommand, IRequest<Response<long?>>
{
    public long Id { get; set; }

    public UpdatePersonCommand(long id)
    {
        Id = id;
    }
}

// Validator
public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{

    public UpdatePersonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.Surname1)
            .NotEmpty().WithMessage("El camp no pot ser buid.");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Text must be not empty")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage("Màxim 15 caràcters");
    }
}

public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, Response<long?>>
{
    private readonly IPeopleRepository _peopleRepo;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepo;
    private readonly ICoursesRepository _coursesRespository;

    public UpdatePersonCommandHandler(IPeopleRepository peopleRepo, IPersonGroupCourseRepository personGroupCourseRepo, ICoursesRepository coursesRespository)
    {
        _peopleRepo = peopleRepo;
        _personGroupCourseRepo = personGroupCourseRepo;
        _coursesRespository = coursesRespository;
    }

    public async Task<Response<long?>> Handle(UpdatePersonCommand request, CancellationToken ct)
    {
        Course c = await _coursesRespository.GetCurrentCoursAsync(ct);
        Person? p = await _peopleRepo.GetByIdAsync(request.Id, ct);

        if (p == null)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "Bad request from UpdatePersonCommand");
        }

        Person? p1 = await _peopleRepo.GetPersonByDocumentIdAsync(request.DocumentId, ct);
        if (p1 != null && p.Id != p1.Id)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, nameof(p.DocumentId), "Ja existeix una persona amb aquest DNI");
        }

        if (request.AcademicRecordNumber.HasValue)
        {
            Person? p2 = await _peopleRepo.GetPersonByAcademicRecordAsync(request.AcademicRecordNumber.Value, ct);
            if (p2 != null && p.Id != p2.Id)
            {
                return Response<long?>.Error(ResponseCode.BadRequest, nameof(request.AcademicRecordNumber), "Ja existeix una persona amb aquest numero d'expedient");
            }
        }

        p.Name = request.Name;
        p.ContactPhone = request.ContactPhone;
        p.DocumentId = request.DocumentId;
        p.Surname1 = request.Surname1;
        p.Surname2 = request.Surname2;
        p.AcademicRecordNumber = request.AcademicRecordNumber;
        
        await _peopleRepo.UpdateAsync(p, CancellationToken.None);

        // Update / create group
        PersonGroupCourse? pgc = await _personGroupCourseRepo.GetCoursePersonGroupById(p.Id, c.Id, CancellationToken.None);
        if (pgc == null && request.GroupId.HasValue)
        {
            pgc = new PersonGroupCourse()
            {
                CourseId = c.Id,
                GroupId = request.GroupId.Value,
                PersonId = p.Id,
                Amipa = request.Amipa,
                Enrolled = request.Enrolled,
            };
            await _personGroupCourseRepo.InsertAsync(pgc, CancellationToken.None);
        }
        else if (pgc != null && request.GroupId.HasValue)
        {
            pgc.GroupId = request.GroupId.Value;
            pgc.Amipa = request.Amipa;
            if (!request.Enrolled)
            {
                pgc.EnrollmentEvent = null;
                pgc.EnrollmentEventId = null;
            }
            pgc.Enrolled = request.Enrolled;


            pgc.SubjectsInfo = request.SubjectsInfo;
            await _personGroupCourseRepo.UpdateAsync(pgc, CancellationToken.None);
        }
        else if (pgc != null && !request.GroupId.HasValue)
        {
            await _personGroupCourseRepo.DeleteAsync(pgc, CancellationToken.None);
        }

        return Response<long?>.Ok(p.Id);
    }
}