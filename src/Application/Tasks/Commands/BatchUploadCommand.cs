using System.Data;
using Application.Common.Services;
using Domain.People;
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
    public static readonly Dictionary<string, Type> _columns = new Dictionary<string, Type>()
    {
        { "expedient", typeof(string) },
        { "dni", typeof(string) },
        { "nom", typeof(string) },
        { "llinatge1", typeof(string) },
        { "llinatge2", typeof(string) },
        { "prematricula", typeof(string) },
        { "grup", typeof(string) },
        { "amipa", typeof(string) },
        { "assig1", typeof(string) },
        { "assig2", typeof(string) },
        { "assig3", typeof(string) },
        { "assig4", typeof(string) },
        { "assig5", typeof(string) },
        { "assig6", typeof(string) },
        { "assig7", typeof(string) },
        { "assig8", typeof(string) },
        { "assig9", typeof(string) },
        { "assig10", typeof(string) },
        { "assig11", typeof(string) },
        { "assig12", typeof(string) },
    };
    
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
        DataTable? dt = _csvParser.Parse(request.file, _columns);
        if (dt == null)
            throw new Exception("todo return bad error");

        var sudents = await ProcessStudents(dt, ct);

        // program logic
        return 1;
    }

    private async Task<IEnumerable<Student>> ProcessStudents(DataTable dt, CancellationToken ct)
    {
        Dictionary<long, DataRow> studentRows = new Dictionary<long, DataRow>(dt.Rows.Count);
        foreach (DataRow r in dt.Rows)
        {
            string? val = r["expedient"].ToString();
            if (string.IsNullOrEmpty(val)) continue;
            long expedient = long.Parse(val.Trim());
            studentRows.Add(expedient, r);
        }

        IEnumerable<long> expedients = studentRows.Select(x => x.Key);
        IEnumerable<Student> existingStudents = await _peopleService.GetManyStudentsAsync(expedients, true, ct);
        foreach (var s in existingStudents)
        {
            if (studentRows.ContainsKey(s.AcademicRecordNumber))
            {
                // update student
                UpdateStudentFromDt(s, studentRows[s.AcademicRecordNumber]);
                studentRows.Remove(s.AcademicRecordNumber);
            }
        }

        // Here in the dict we have new users to be added
        IEnumerable<Student> studentsToAdd = studentRows.Select(x => CreateStudentFromDt(x.Key, x.Value));

        return existingStudents.Concat(studentsToAdd);

    }

    private Student CreateStudentFromDt(long academincRecordNumber, DataRow row)
    {
        var s = new Student();
        s.AcademicRecordNumber = academincRecordNumber;
        UpdateStudentFromDt(s, row);
        return s;
    }

    private void UpdateStudentFromDt(Student s, DataRow row)
    {
        s.Person.Name = row["nom"].ToString() ?? "";
        s.Person.DocumentId = row["dni"].ToString() ?? "";
        s.Person.Surname1 = row["llinatge1"].ToString() ?? "";
        s.Person.Surname2 = row["llinatge2"].ToString() ?? "";
        s.Person.ContactMail = row["prematricula"].ToString() ?? "";
        s.Person.ContactPhone = row["grup"].ToString() ?? "";
        s.SubjectsInfo = row[""].ToString() ?? "";
    }

    public record UploadPeopleTransaction(IEnumerable<Student> students);
}