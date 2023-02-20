using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record PeopleBatchUploadCommand(Stream File) : IRequest<Response<BatchUploadVm>>;

// Validator for the model

// Optionally define a view model
public record BatchUploadSummary(int GroupsCreated, int PeopleCreated, int PeopleUpdated);
public record BatchUploadVm(BatchUploadSummary Data);

// Handler
public class BatchUploadCommandHandler : IRequestHandler<PeopleBatchUploadCommand, Response<BatchUploadVm>>
{
    #region props

    private readonly ICsvParser _csvParser;
    private readonly IPeopleRepository _peopleRepo;
    private readonly ICoursesRepository _coursesRepo;
    private readonly IGroupsRepository _groupsRepo;
    private readonly IStudentsRepository _studentsRepo;
    private readonly ITransactionsService _transactionsService;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepo;

    public BatchUploadCommandHandler(
        ICsvParser csvParser,
        IPeopleRepository peopleRepo,
        ICoursesRepository coursesRepo,
        IGroupsRepository groupsRepo,
        IStudentsRepository studentsRepo,
        IPersonGroupCourseRepository personGroupCourseRepo,
        ITransactionsService transactionsService)
    {
        _csvParser = csvParser;
        _peopleRepo = peopleRepo;
        _coursesRepo = coursesRepo;
        _groupsRepo = groupsRepo;
        _transactionsService = transactionsService;
        _studentsRepo = studentsRepo;
        _personGroupCourseRepo = personGroupCourseRepo;
    }
    #endregion

    public async Task<Response<BatchUploadVm>> Handle(PeopleBatchUploadCommand request, CancellationToken ct)
    {
        // Parse csv
        var result = _csvParser.ParseBatchUpload(request.File);
        request.File.Dispose();

        if (result.Values == null) return Response<BatchUploadVm>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? "Error processing csv.");

        IEnumerable<BatchUploadRowModel> rows = result.Values;

        // Process groups
        IDictionary<string, Group> groups = await ProcessGroups(rows, ct);
        // Process students
        IDictionary<long, Student> students = await ProcessStudents(rows.Where(x => x.Expedient.HasValue), ct);
        // Process people
        IDictionary<string, Person> people = await ProcessPeople(rows.Where(x => !x.Expedient.HasValue), ct);
        
        foreach (var s in students)
        {
            people[s.Value.DocumentId] = s.Value;
        }

        // Process PersonGroupCourse
        IDictionary<string, PersonGroupCourse> presonGroupCourses = await ProcessPersonGroupCourse(people, groups, rows, ct);

        var m = new BatchUploadModel(people, groups, presonGroupCourses.Values);
        var summary = new BatchUploadSummary(m.NewGroups.Count(), m.NewPeople.Count(), m.ExistingPeople.Count());

        await _transactionsService.InsertAndUpdateTransactionAsync(m, ct);
        return Response<BatchUploadVm>.Ok(new BatchUploadVm(summary));
    }

    #region private methods

    private async Task<IDictionary<string, PersonGroupCourse>> ProcessPersonGroupCourse(
            IDictionary<string, Person> people,
            IDictionary<string, Group> groups,
            IEnumerable<BatchUploadRowModel> rows,
            CancellationToken ct)
    {
        var course = await _coursesRepo.GetCurrentCoursAsync(ct);
        var peopleIds = people.Select(x => x.Value.Id);
        var personGroupCourse = (await _personGroupCourseRepo.GetCurrentCourseGroupByPeopleIdsAsync(peopleIds, ct)).ToDictionary(x => x.Person.DocumentId, x => x);
        foreach (var r in rows)
        {
            Person p = people[r.Identitat];

            if (string.IsNullOrEmpty(r.Grup))
            {
                // no group. Should add the default group.
            }
            else
            {
                Group g = groups[r.Grup];

                PersonGroupCourse pgc;
                if (personGroupCourse.ContainsKey(p.DocumentId))
                {
                    pgc = personGroupCourse[p.DocumentId];
                    if (pgc.Group.Id != g.Id)
                    {
                        pgc.Group = g;
                    }
                }
                else
                {
                    pgc = new PersonGroupCourse()
                    {
                        Group = g,
                        Person = p,
                        Course = course,
                    };
                    personGroupCourse.Add(p.DocumentId, pgc);
                }
            }
        }
        return personGroupCourse;
    }

    private async Task<IDictionary<string, Group>> ProcessGroups(IEnumerable<BatchUploadRowModel> rows, CancellationToken ct)
    {
        IEnumerable<string> groupNames = rows.Where(x => !string.IsNullOrEmpty(x.Grup)).Select(x => x.Grup ?? "").Distinct();
        IEnumerable<Group> existingGroups = await _groupsRepo.GetGroupsByNameAsync(groupNames, ct);
        IDictionary<string, Group> groups = existingGroups.ToDictionary(x => x.Name, x => x);

        foreach (var name in groupNames)
        {
            if (groups.ContainsKey(name)) continue;

            var g = new Group()
            {
                Name = name,
                Created = DateTimeOffset.UtcNow,
            };
            groups[name] = g;
        }
        return groups;
    }

    private async Task<IDictionary<string, Person>> ProcessPeople(IEnumerable<BatchUploadRowModel> rows, CancellationToken ct)
    {
        IEnumerable<Person> existingPeople = await _peopleRepo.GetPeopleByDocumentIdsAsync(rows.Select(x => x.Identitat), ct);
        IDictionary<string, Person> people = existingPeople.ToDictionary(x => x.DocumentId, x => x);
        foreach (var r in rows)
        {
            if (people.ContainsKey(r.Identitat))
            {
                var existingPerson = people[r.Identitat];
                var newPerson = CreatePersonFromRow(r);
                UpdatePersonFields(existingPerson, newPerson);
            }
            else
            {
                var p = CreatePersonFromRow(r);
                people[r.Identitat] = p;
            }
        }
        return people;
    }

    private async Task<IDictionary<long, Student>> ProcessStudents(IEnumerable<BatchUploadRowModel> rows, CancellationToken ct)
    {
        IEnumerable<Student> existingStudents = await _studentsRepo.GetStudentsByAcademicRecordAsync(rows.Select(x => x.Expedient ?? 0), ct);
        IDictionary<long, Student> students = existingStudents.ToDictionary(x => x.AcademicRecordNumber, x => x);
        foreach (var r in rows)
        {
            var number = r.Expedient ?? 0;
            if (students.ContainsKey(number))
            {
                var existingStudent = students[number];
                var newStudent = CreateStudentFromRow(r);
                UpdateStudentFields(existingStudent, newStudent);
            }
            else
            {
                var s = CreateStudentFromRow(r);
                students[number] = s;
            }
        }
        return students;
    }

    private Student CreateStudentFromRow(BatchUploadRowModel po)
    {
        if (po.Expedient.HasValue)
        {
            var s = new Student()
            {
                AcademicRecordNumber = po.Expedient.Value,
                Amipa = false,
                SubjectsInfo = po.Assignatures,
                PreEnrollment = po.Prematricula == 1,
            };

            var tempPerson = CreatePersonFromRow(po);
            UpdatePersonFields(s, tempPerson);

            return s;
        }
        throw new ArgumentException("Check argument is a student");
    }

    private void UpdateStudentFields(Student s, Student newStudent)
    {
        s.SubjectsInfo = newStudent.SubjectsInfo;
        s.Amipa = newStudent.Amipa;
        s.PreEnrollment = newStudent.PreEnrollment;
        UpdatePersonFields(s, newStudent);
    }

    private void UpdatePersonFields(Person s, Person newPerson)
    {
        s.DocumentId = newPerson.DocumentId;
        s.Name = newPerson.Name;
        s.Surname1 = newPerson.Surname1;
        s.Surname2 = newPerson.Surname2;
        s.ContactMail = newPerson.ContactMail;
        s.ContactPhone = newPerson.ContactPhone;
    }

    private Person CreatePersonFromRow(BatchUploadRowModel row)
    {
        var p = new Person()
        {
            DocumentId = row.Identitat,
            ContactMail = row.EmailContacte,
            ContactPhone = row.TelContacte,
            Name = row.Nom,
            Surname1 = row.Llinatge1,
            Surname2 = row.Llinatge2,
        };
        return p;
    }
    #endregion
}