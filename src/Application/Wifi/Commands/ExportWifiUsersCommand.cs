using Application.Common;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record ExportWifiUsersCommand() : IRequest<Response<ExportWifiUsersVm>>;

// Validator for the model

// Optionally define a view model
public record ExportWifiUsersVm();

// Handler
public class ExportWifiUsersHandler : IRequestHandler<ExportWifiUsersCommand, Response<ExportWifiUsersVm>>
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

    public ExportWifiUsersHandler(ICsvParser csvParser, IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository, IPeopleRepository peopleRepository, IConfiguration configuration)
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


    public async Task<Response<ExportWifiUsersVm>> Handle(ExportWifiUsersCommand request, CancellationToken ct)
    {
        Course course = await _courseRepository.GetCurrentCoursAsync(ct);

        var now = DateTimeOffset.UtcNow;
        string filePath = $"{tempFolderPath}export_wifi_{now.Date.Year}{now.Date.Month}{now.Date.Day}{now.DateTime.Hour}{now.DateTime.Second}.csv";

        await _csvParser.WriteHeadersAsync<WifiAccountRow>(filePath);


        IEnumerable<PersonGroupCourse> pgcs = _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(course.Id, ct);

        foreach (var pgc in pgcs)
        {
            Person p = pgc.Person;
            string password = Common.Helpers.GenerateString.RandomAlphanumeric(8);

            if (!string.IsNullOrEmpty(p.ContactMail))
            {
                var ac = new WifiAccountRow()
                {
                    Email = p.ContactMail,
                    Password = password,
                };

                await _csvParser.WriteToFileAsync(filePath, ac, false);
            }
        }

        return Response<ExportWifiUsersVm>.Ok(new ExportWifiUsersVm());
    }
}
