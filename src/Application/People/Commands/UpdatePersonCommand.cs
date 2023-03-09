using Application.Common;
using Application.Common.Services;
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

        RuleFor(x => x.GroupId)
            .NotNull().NotEmpty().WithMessage("Com a mínim s'ha d'especificar un group ");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Text must be not empty")
            .MaximumLength(50).WithMessage("Màxim 50 caràcters");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage("Màxim 15 caràcters");

        RuleFor(x => x.ContactMail)
            .MaximumLength(50).WithMessage("Màxim 100 caràcters");
    }
}

public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, Response<long?>>
{
    private readonly IPeopleRepository _peopleRepo;
    private readonly IStudentsRepository _studentRepo;

    public UpdatePersonCommandHandler(
    IPeopleRepository peopleRepo,
    IStudentsRepository studentRepo)
    {
        _peopleRepo = peopleRepo;
        _studentRepo = studentRepo;
    }
    public async Task<Response<long?>> Handle(UpdatePersonCommand request, CancellationToken ct)
    {
        //Comprovam que no existeix un usuari amb un DocumentID o Academic Record Number igual a la BBDD que no sigui l'usuari que volem actualitzar.


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
            Person? p2 = await _studentRepo.GetStudentByAcademicRecordAsync(request.AcademicRecordNumber.Value, ct);
            if (p2 != null && p.Id != p2.Id)
            {
                return Response<long?>.Error(ResponseCode.BadRequest, nameof(request.AcademicRecordNumber), "Ja existeix una persona amb aquest numero d'expedient");
            }
            Student? s = p as Student;
            if (s != null)
            {
                //Actualitzar estudiant
                s.AcademicRecordNumber = request.AcademicRecordNumber.Value;
                s.SubjectsInfo = request.SubjectsInfo;
                p = s;
            }
            else
            {
                //Crear estudiant
                s = new Student();
                s.AcademicRecordNumber = request.AcademicRecordNumber.Value;
                s.SubjectsInfo = request.SubjectsInfo;
                s.DocumentId = request.DocumentId;
                s.Name = request.Name;
                s.Surname1 = request.Surname1;
                
                //Falta crear estudiant i enllaçar estudiant amb persona que ja existeix
                //await _studentRepo.AddStudentsExistingPersonAsync(s,p,ct);
                //await _peopleRepo.UpdateAsync(p, ct);

            }
        }
        else
        {
            if (p is Student)
            {
                //Eliminar estudiant
            }
        }

        //Actualitzar P
        

        return Response<long?>.Ok(p.Id);
    }
}