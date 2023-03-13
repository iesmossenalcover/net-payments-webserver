using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record PeopleBatchUploadCommand(Stream File) : IRequest<Response<BatchUploadSummary>>;

// Validator for the model

// Optionally define a view model
public record BatchUploadSummary(int GroupsCreated, int PeopleCreated, int PeopleUpdated);

// Handler
public class BatchUploadCommandHandler : IRequestHandler<PeopleBatchUploadCommand, Response<BatchUploadSummary>>
{
    #region props

    private readonly ICsvParser _csvParser;
    private readonly IPeopleRepository _peopleRepo;
    private readonly ICoursesRepository _coursesRepo;
    private readonly IGroupsRepository _groupsRepo;
    private readonly ITransactionsService _transactionsService;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepo;

    public BatchUploadCommandHandler(
        ICsvParser csvParser,
        IPeopleRepository peopleRepo,
        ICoursesRepository coursesRepo,
        IGroupsRepository groupsRepo,
        IPersonGroupCourseRepository personGroupCourseRepo,
        ITransactionsService transactionsService)
    {
        _csvParser = csvParser;
        _peopleRepo = peopleRepo;
        _coursesRepo = coursesRepo;
        _groupsRepo = groupsRepo;
        _transactionsService = transactionsService;
        _personGroupCourseRepo = personGroupCourseRepo;
    }
    #endregion

    public async Task<Response<BatchUploadSummary>> Handle(PeopleBatchUploadCommand request, CancellationToken ct)
    {
        // Parse csv
        var result = _csvParser.ParseBatchUpload(request.File);
        request.File.Dispose();

        if (result.Values == null) return Response<BatchUploadSummary>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? "Error processing csv.");

        IEnumerable<BatchUploadRowModel> rows = result.Values;

        // Process groups
        IDictionary<string, Group> groups = await ProcessGroups(rows, ct);
        // Process people
        IDictionary<string, Person> people = await ProcessPeople(rows, ct);
        // Process PersonGroupCourse
        IDictionary<string, PersonGroupCourse> presonGroupCourses = await ProcessPersonGroupCourse(people, groups, rows, ct);

        var m = new BatchUploadModel(people, groups, presonGroupCourses.Values);
        var summary = new BatchUploadSummary(m.NewGroups.Count(), m.NewPeople.Count(), m.ExistingPeople.Count());

        await _transactionsService.InsertAndUpdateTransactionAsync(m, ct);
        return Response<BatchUploadSummary>.Ok(summary);
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
                        pgc.SubjectsInfo = r.Assignatures;
                    }
                }
                else
                {
                    pgc = new PersonGroupCourse()
                    {
                        Group = g,
                        Person = p,
                        Course = course,
                        Amipa = false,
                        Enrolled = false,
                        SubjectsInfo = r.Assignatures,
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
                Person p = people[r.Identitat];
                p.AcademicRecordNumber = r.Expedient;
                p.ContactMail = r.EmailContacte;
                p.ContactPhone = r.TelContacte;
                p.DocumentId = r.Identitat;
                p.Name = r.Nom;
                p.Surname1 = r.Llinatge1;
                p.Surname2 = r.Llinatge2;
            }
            else
            {
                Person p = new Person()
                {
                    AcademicRecordNumber = r.Expedient,
                    ContactMail = r.EmailContacte,
                    ContactPhone = r.TelContacte,
                    DocumentId = r.Identitat,
                    Name = r.Nom,
                    Surname1 = r.Llinatge1,
                    Surname2 = r.Llinatge2,
                };
                people[r.Identitat] = p;
            }
        }
        return people;
    }
    #endregion
}