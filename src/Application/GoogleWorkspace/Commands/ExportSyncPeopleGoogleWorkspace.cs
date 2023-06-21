using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record ExportSyncPeopleGoogleWorkspaceCommand() : IRequest<Response<ExportSyncPeopleGoogleWorkspaceVm>>;

// Validator for the model

// Optionally define a view model
public record ExportSyncPeopleGoogleWorkspaceVm();

// Handler
public class ExportSyncPeopleGoogleWorkspaceHandler : IRequestHandler<ExportSyncPeopleGoogleWorkspaceCommand, Response<ExportSyncPeopleGoogleWorkspaceVm>>
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
    private readonly string tempFolderPath;

    public ExportSyncPeopleGoogleWorkspaceHandler(ICsvParser csvParser, IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository, IPeopleRepository peopleRepository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _csvParser = csvParser;
        emailDomain = configuration.GetValue<string>("GoogleApiDomain") ?? throw new Exception("GoogleApiDomain");
        tempFolderPath = configuration.GetValue<string>("TempFolderPath") ?? throw new Exception("TempFolderPath");
        excludeEmails = configuration.GetSection("GoogleApiExcludeAccounts").Get<string[]>() ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion


    public async Task<Response<ExportSyncPeopleGoogleWorkspaceVm>> Handle(ExportSyncPeopleGoogleWorkspaceCommand request, CancellationToken ct)
    {
        Course course = await _courseRepository.GetCurrentCoursAsync(ct);
        IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);

        var now = DateTimeOffset.UtcNow;
        string filePath = $"{tempFolderPath}export_users_{now.Date.Year}{now.Date.Month}{now.Date.Day}{now.DateTime.Hour}{now.DateTime.Second}.csv";

        await _csvParser.WriteHeadersAsync<AccountRow>(filePath);

        foreach (var ou in ouRelations)
        {
            IEnumerable<PersonGroupCourse> pgcs = await _personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, ou.GroupId, ct);

            foreach (var pgc in pgcs)
            {
                Person p = pgc.Person;
                bool newAccount = string.IsNullOrEmpty(p.ContactMail);
                string email = string.IsNullOrEmpty(p.ContactMail) ? SyncPersonToGoogleWorkspaceCommandHandler.GetEmail(p, emailDomain) : p.ContactMail;

                // IMPORTANT: Exclude members
                if (excludeEmails.Contains(p.ContactMail)) continue;

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

                await _csvParser.WriteToFileAsync(filePath, ac, false);
            }
            await _peopleRepository.UpdateManyAsync(pgcs.Select(x => x.Person), CancellationToken.None);
        }

        return Response<ExportSyncPeopleGoogleWorkspaceVm>.Ok(new ExportSyncPeopleGoogleWorkspaceVm());
    }
}



public class AccountRow
{
    public string First { get; set; } = string.Empty;
    public string Last { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Org { get; set; } = string.Empty;
    public string New { get; set; } = string.Empty;
    public string Recovery { get; set; } = string.Empty;
    public string Home { get; set; } = string.Empty;
    public string Work { get; set; } = string.Empty;
    public string RecoveryPhone { get; set; } = string.Empty;
    public string WorkPhone { get; set; } = string.Empty;
    public string HomePhone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string WorkAddress { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public string Employee { get; set; } = string.Empty;
    public string EmployeeType { get; set; } = string.Empty;
    public string EmployeeTitle { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Cost { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string FloorSection { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Advanced { get; set; } = string.Empty;
}

