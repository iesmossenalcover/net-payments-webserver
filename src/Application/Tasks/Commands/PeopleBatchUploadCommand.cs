using System.Data;
using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using FluentValidation;
using MediatR;

namespace Application.Tasks.Commands;

// Model we receive
public record PeopleBatchUploadCommand(Stream File) : IRequest<Response<PeopleBatchUploadSummary>>;

// Validator for the model

// Optionally define a view model
public record PeopleBatchUploadSummary(int GroupsCreated, int PeopleCreated, int PeopleUpdated);

// Handler
public class PeopleBatchUploadCommandHandler : IRequestHandler<PeopleBatchUploadCommand, Response<PeopleBatchUploadSummary>>
{
    #region props

    private readonly ICsvParser _csvParser;
    private readonly IPeopleRepository _peopleRepo;
    private readonly ICoursesRepository _coursesRepo;
    private readonly IGroupsRepository _groupsRepo;
    private readonly ITransactionsService _transactionsService;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepo;

    public PeopleBatchUploadCommandHandler(
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

    public async Task<Response<PeopleBatchUploadSummary>> Handle(PeopleBatchUploadCommand request, CancellationToken ct)
    {
        // Parse csv
        var result = _csvParser.Parse<BatchUploadRow>(request.File);
        request.File.Dispose();

        if (result.Values == null) return Response<PeopleBatchUploadSummary>.Error(ResponseCode.BadRequest, "Error processing csv.");

        IEnumerable<BatchUploadRow> rows = result.Values;

        // fix important data.
        foreach (var r in rows)
        {
            r.Identitat = r.Identitat.ToUpper();
        }

        // Process groups
        IDictionary<string, Group> groups = await ProcessGroups(rows, ct);
        // Process people
        IDictionary<string, Person> people = await ProcessPeople(rows, ct);
        // Process PersonGroupCourse
        IDictionary<string, PersonGroupCourse> presonGroupCourses = await ProcessPersonGroupCourse(people, groups, rows, ct);

        var m = new BatchUploadModel(people, groups, presonGroupCourses.Values);
        var summary = new PeopleBatchUploadSummary(m.NewGroups.Count(), m.NewPeople.Count(), m.ExistingPeople.Count());

        await _transactionsService.InsertAndUpdateTransactionAsync(m, ct);
        return Response<PeopleBatchUploadSummary>.Ok(summary);
    }

    #region private methods

    private async Task<IDictionary<string, PersonGroupCourse>> ProcessPersonGroupCourse(
            IDictionary<string, Person> people,
            IDictionary<string, Group> groups,
            IEnumerable<BatchUploadRow> rows,
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
                    pgc.Group = g;
                    pgc.SubjectsInfo = r.Assignatures;
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

    private async Task<IDictionary<string, Group>> ProcessGroups(IEnumerable<BatchUploadRow> rows, CancellationToken ct)
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

    private async Task<IDictionary<string, Person>> ProcessPeople(IEnumerable<BatchUploadRow> rows, CancellationToken ct)
    {
        IEnumerable<Person> existingPeople = await _peopleRepo.GetPeopleByDocumentIdsAsync(rows.Select(x => x.Identitat), ct);
        IDictionary<string, Person> people = existingPeople.ToDictionary(x => x.DocumentId, x => x);
        foreach (var r in rows)
        {
            if (people.ContainsKey(r.Identitat.Trim()))
            {
                Person p = people[r.Identitat];
                p.AcademicRecordNumber = r.Expedient;
                p.ContactPhone = r.TelContacte;
                p.DocumentId = r.Identitat.Trim();
                p.Name = r.Nom.Trim();
                p.Surname1 = r.Llinatge1.Trim();
                p.Surname2 = r.Llinatge2 != null ? r.Llinatge2.Trim() : null;
            }
            else
            {
                Person p = new Person()
                {
                    AcademicRecordNumber = r.Expedient,
                    ContactPhone = r.TelContacte,
                    DocumentId = r.Identitat.Trim(),
                    Name = r.Nom.Trim(),
                    Surname1 = r.Llinatge1.Trim(),
                    Surname2 = r.Llinatge2 != null ? r.Llinatge2.Trim() : null,
                };
                people[r.Identitat] = p;
            }
        }
        return people;
    }
    #endregion
}