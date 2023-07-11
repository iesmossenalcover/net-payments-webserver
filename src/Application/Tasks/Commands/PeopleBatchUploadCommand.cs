using System.Data;
using Application.Common;
using Application.Common.Models;
using Domain.Services;
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

        if (result.Values == null) return Response<PeopleBatchUploadSummary>.Error(ResponseCode.BadRequest, result.ErrorMessage ?? "Error processing csv.");

        IEnumerable<BatchUploadRow> rows = result.Values;

        // fix important data.
        foreach (var r in rows)
        {
            r.DocumentId = r.DocumentId.ToUpper();
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
            Person p = people[r.DocumentId];

            if (string.IsNullOrEmpty(r.GroupName))
            {
                // no group. Should add the default group.
            }
            else
            {
                Group g = groups[r.GroupName];

                PersonGroupCourse pgc;
                if (personGroupCourse.ContainsKey(p.DocumentId))
                {
                    pgc = personGroupCourse[p.DocumentId];
                    pgc.Group = g;
                    pgc.SubjectsInfo = r.Subjects;

                    // If amipa field is set, then update.
                    if (r.IsAmipa.HasValue)
                    {
                        pgc.Amipa = r.IsAmipa.Value;
                        pgc.AmipaDate = r.IsAmipa.Value ? DateTimeOffset.UtcNow : null;
                    }

                    // If enrolled field is set, then update.
                    if (r.Enrolled.HasValue)
                    {
                        pgc.Enrolled = r.Enrolled.Value;
                        pgc.EnrolledDate = r.Enrolled.Value ? DateTimeOffset.UtcNow : null;
                        pgc.EnrollmentEventId = null;
                        pgc.EnrollmentEvent = null;
                    }
                }
                else
                {
                    pgc = new PersonGroupCourse()
                    {
                        Group = g,
                        Person = p,
                        Course = course,
                        Amipa = r.IsAmipa ?? false,
                        AmipaDate = r.IsAmipa.HasValue && r.IsAmipa.Value ? DateTimeOffset.UtcNow : null,
                        Enrolled = r.Enrolled ?? false,
                        EnrolledDate = r.Enrolled.HasValue && r.Enrolled.Value ? DateTimeOffset.UtcNow : null,
                        SubjectsInfo = r.Subjects,
                    };
                    personGroupCourse.Add(p.DocumentId, pgc);
                }
            }
        }
        return personGroupCourse;
    }

    private async Task<IDictionary<string, Group>> ProcessGroups(IEnumerable<BatchUploadRow> rows, CancellationToken ct)
    {
        IEnumerable<string> groupNames = rows.Where(x => !string.IsNullOrEmpty(x.GroupName)).Select(x => x.GroupName ?? "").Distinct();
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
        IEnumerable<Person> existingPeople = await _peopleRepo.GetPeopleByDocumentIdsAsync(rows.Select(x => x.DocumentId), ct);
        IDictionary<string, Person> people = existingPeople.ToDictionary(x => x.DocumentId, x => x);
        foreach (var r in rows)
        {
            if (people.ContainsKey(r.DocumentId.Trim()))
            {
                Person p = people[r.DocumentId];
                p.AcademicRecordNumber = r.AcademicRecordNumber;
                p.ContactPhone = r.ContactPhone;
                p.DocumentId = r.DocumentId.Trim();
                p.Name = r.FirstName.Trim();
                p.Surname1 = r.Surname1.Trim();
                p.Surname2 = r.Surname2 != null ? r.Surname2.Trim() : null;
            }
            else
            {
                Person p = new Person()
                {
                    AcademicRecordNumber = r.AcademicRecordNumber,
                    ContactPhone = r.ContactPhone,
                    DocumentId = r.DocumentId.Trim(),
                    Name = r.FirstName.Trim(),
                    Surname1 = r.Surname1.Trim(),
                    Surname2 = r.Surname2 != null ? r.Surname2.Trim() : null,
                };
                people[r.DocumentId] = p;
            }
        }
        return people;
    }
    #endregion
}