using System.Data;
using Application.Common.Services;
using Domain.Entities.People;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record PeopleBatchUploadCommand(Stream file) : IRequest<long>;

// Validator for the model

// Optionally define a view model

// Handler
public class BatchUploadCommandHandler : IRequestHandler<PeopleBatchUploadCommand, long>
{
    #region props
    
    /* csv structure
    Expedient,Identitat,Nom,Llinatge1,Llinatge2,EmailContacte,TelContacte,Prematricula,Pagament,Grup,Amipa,Assignatures
    1,42374617S,Belinda Elisabeth,Arias,Tarira,prova@ies.com,6741222554,1,0,1ESOC,0,
    */
    
    private readonly ICsvParser _csvParser;
    private readonly IPeopleService _peopleService;

    public BatchUploadCommandHandler(ICsvParser csvParser, IPeopleService peopleService)
    {
        _csvParser = csvParser;
        _peopleService = peopleService;
    }
    #endregion

    public async Task<long> Handle(PeopleBatchUploadCommand request, CancellationToken ct)
    {
        // Parse csv
        IEnumerable<PeopleObject>? rows = _csvParser.Parse<PeopleObject>(request.file);
        request.file.Dispose();

        if (rows == null)
        {
            throw new Exception("return bad request");
        }

        // Process groups
        // Process students
        IEnumerable<Student> students = await ProcessStudents(rows.Where(x => x.Expedient.HasValue), ct);
        // persons
        // Process PersonGroupCourse

        // Persist in one transaction
        return 1;
    }

    private async Task<IEnumerable<Student>> ProcessStudents(IEnumerable<PeopleObject> rows, CancellationToken ct)
    {
        Dictionary<long, Student> students = new Dictionary<long, Student>(rows.Count());
        foreach (PeopleObject po in rows)
        {
            Student? s = CreateStudentFromRow(po);
            if (s == null)
            {
                throw new Exception("TODO: add mor info, invalid row");
            }
            students.Add(s.AcademicRecordNumber, s);
        }

        IEnumerable<long> expedients = students.Select(x => x.Key);
        IEnumerable<Student> existingStudents = await _peopleService.GetManyStudentsAsync(expedients, true, ct);
        foreach (var s in existingStudents)
        {
            if (students.ContainsKey(s.AcademicRecordNumber))
            {
                // update student
                UpdateStudentFields(s, students[s.AcademicRecordNumber]);
                students.Remove(s.AcademicRecordNumber);
            }
        }

        return existingStudents.Concat(students.Select(x => x.Value));

    }

    private void UpdateStudentFields(Student s, Student newStudent)
    {
        s.SubjectsInfo = newStudent.SubjectsInfo;
        s.Person.DocumentId = newStudent.Person.DocumentId;
        s.Person.Name = newStudent.Person.Name;
        s.Person.Surname1 = newStudent.Person.Surname1;
        s.Person.Surname2 = newStudent.Person.Surname2;
        s.Person.ContactMail = newStudent.Person.ContactMail;
        s.Person.ContactPhone = newStudent.Person.ContactPhone;
    }

    private Student? CreateStudentFromRow(PeopleObject po)
    {
        var s = new Student();
        if (!po.Expedient.HasValue)
        {
             throw new Exception("this is not and student");
        }
        s.AcademicRecordNumber = po.Expedient.Value;
        s.Person = new Person();
        s.Person.DocumentId = po.Identitat;
        s.Person.Name = po.Nom;
        s.Person.Surname1 = po.Llinatge1;
        s.Person.Surname2 =po.Llinatge2;
        s.Person.ContactMail = po.EmailContacte;
        s.Person.ContactPhone = po.TelContacte;
        s.SubjectsInfo = po.Assignatures;
        return s;
    }
}