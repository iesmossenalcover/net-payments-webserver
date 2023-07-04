using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record ExportSyncPeopleGoogleWorkspaceCommand() : IRequest<FileVm>;

// Handler
public class ExportSyncPeopleGoogleWorkspaceHandler : IRequestHandler<ExportSyncPeopleGoogleWorkspaceCommand, FileVm>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly ICoursesRepository _courseRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly ICsvParser _csvParser;
    private readonly string emailDomain;
    private readonly string[] excludeEmails;

    public ExportSyncPeopleGoogleWorkspaceHandler(ICsvParser csvParser, IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository, IPeopleRepository peopleRepository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _csvParser = csvParser;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion


    public async Task<FileVm> Handle(ExportSyncPeopleGoogleWorkspaceCommand request, CancellationToken ct)
    {
        Course course = await _courseRepository.GetCurrentCoursAsync(ct);
        IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);

        var now = DateTimeOffset.UtcNow;
        string fileName = $"export_users_{now.Date.Year}{now.Date.Month}{now.Date.Day}{now.DateTime.Hour}{now.DateTime.Second}.csv";

        List<AccountRow> rows = new List<AccountRow>();
        foreach (var ou in ouRelations)
        {
            IEnumerable<PersonGroupCourse> pgcs = await _personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, ou.GroupId, ct);

            foreach (var pgc in pgcs)
            {
                Person p = pgc.Person;
                bool newAccount = string.IsNullOrEmpty(p.ContactMail);
                string email = string.IsNullOrEmpty(p.ContactMail) ? SyncPersonToGoogleWorkspaceCommandHandler.GetEmail(p, emailDomain) : p.ContactMail;

                // IMPORTANT: Exclude members
                if (excludeEmails.Contains(email)) continue;

                string password = "****";
                bool change = false;
                if (newAccount || ou.UpdatePassword)
                {
                    password = Common.Helpers.GenerateString.RandomAlphanumeric(8);
                    change = true;
                }

                var ac = new AccountRow()
                {
                    First = p.Name,
                    Last = p.LastName,
                    Email = email,
                    Password = password,
                    Org = ou.ActiveOU,
                    Change = change ? "True" : "False",
                    NewStatus = "Active",
                };

                p.ContactMail = email;
                rows.Add(ac);

            }
            // UNcomment when ready.
            //await _peopleRepository.UpdateManyAsync(pgcs.Select(x => x.Person), CancellationToken.None);
        }

        var memStream = new MemoryStream();
        var streamWriter = new StreamWriter(memStream);
        await _csvParser.WriteToStreamAsync(streamWriter, rows);
        
        return new FileVm(memStream, "text/csv", fileName);
    }
}
