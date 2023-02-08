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
        IDictionary<string, Group> groups = await ProcessGroups(rows, ct);
        // Process students
        IDictionary<long, Student> students = await ProcessStudents(rows.Where(x => x.Expedient.HasValue), ct);
        // Teachers
        IDictionary<string, Person> people = await ProcessPeople(rows.Where(x => !x.Expedient.HasValue), ct);
        // Process PersonGroupCourse
        IDictionary<string, PersonGroupCourse> presonGroupCourses = await ProcessPersonGroupCourse(people, students, groups, rows, ct);

        await _peopleService.InsertAndUpdateTransactionAsync(groups.Values, people.Values, students.Values, presonGroupCourses.Values, ct);

        return 1;
    }

    #region private methods

    private async Task<IDictionary<string, PersonGroupCourse>> ProcessPersonGroupCourse(
            IDictionary<string, Person> people,
            IDictionary<long, Student> students,
            IDictionary<string, Group> groups,
            IEnumerable<PeopleObject> rows,
            CancellationToken ct)
        {
            var course = await _peopleService.GetCurrentCoursAsync(ct);
            var peopleIds = people.Select(x => x.Value.Id).Concat(students.Select(y => y.Value.PersonId));
            var personGroupCourse = (await _peopleService.GetCurrentCoursePersonGroupByPeopleIdsAsync(peopleIds, ct)).ToDictionary(x => x.Person.DocumentId, x => x);
            foreach (var r in rows)
            {
                Person p = r.Expedient.HasValue ? 
                                students[r.Expedient.Value].Person : 
                                people[r.Identitat];

                if (string.IsNullOrEmpty(r.Grup))
                {
                    // no te grup a excel, hauriem d'eliminar l'associació
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

        private async Task<IDictionary<string, Group>> ProcessGroups(IEnumerable<PeopleObject> rows, CancellationToken ct)
    {
        IEnumerable<string> groupNames = rows.Where(x => !string.IsNullOrEmpty(x.Grup)).Select(x => x.Grup ?? "").Distinct();
        IEnumerable<Group> existingGroups = await _peopleService.GetGroupsByNameAsync(groupNames, ct);
        IDictionary<string, Group> groups = existingGroups.ToDictionary(x => x.Name, x => x);

        foreach (var name in groupNames)
        {
            if (groups.ContainsKey(name)) continue;

            var g = new Group()
            {
                Name = name,
                Created = DateTime.Now
            };
            groups[name] = g;
        }
        return groups;
    }

    private async Task<IDictionary<string, Person>> ProcessPeople(IEnumerable<PeopleObject> rows, CancellationToken ct)
    {
        IEnumerable<Person> existingPeople = await _peopleService.GetPeopleAsync(rows.Select(x => x.Identitat), ct);
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

    private async Task<IDictionary<long, Student>> ProcessStudents(IEnumerable<PeopleObject> rows, CancellationToken ct)
    {
        IEnumerable<Student> existingStudents = await _peopleService.GetManyStudentsAsync(rows.Select(x => x.Expedient ?? 0), true, ct);
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
                var p = CreateStudentFromRow(r);
                students[number] = p;
            }
        }
        return students;
    }

    private Student CreateStudentFromRow(PeopleObject po)
    {
        var s = new Student();
        s.AcademicRecordNumber = po.Expedient ?? 0;
        s.Person = CreatePersonFromRow(po);
        s.SubjectsInfo = po.Assignatures;
        return s;
    }

    private void UpdateStudentFields(Student s, Student newStudent)
    {
        s.SubjectsInfo = newStudent.SubjectsInfo;
        UpdatePersonFields(s.Person, newStudent.Person);
    }

    private Person CreatePersonFromRow(PeopleObject po)
    {
        var s = new Person();
        s.Name = po.Nom;
        s.DocumentId = po.Identitat;
        s.Surname1 = po.Llinatge1;
        s.Surname2 = po.Llinatge2;
        s.ContactMail = po.EmailContacte;
        s.ContactPhone = po.TelContacte;
        return s;
    }

    private void UpdatePersonFields(Person s, Person newPerson)
    {
        s.Name = newPerson.Name;
        s.Surname1 = newPerson.Surname1;
        s.Surname2 = newPerson.Surname2;
        s.ContactMail = newPerson.ContactMail;
        s.ContactPhone = newPerson.ContactPhone;
    }
    #endregion
}