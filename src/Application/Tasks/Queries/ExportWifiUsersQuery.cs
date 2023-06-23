using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record ExportWifiUsersQuery() : IRequest<ExportWifiUsersVm>;

// Validator for the model

// Optionally define a view model
public record ExportWifiUsersVm(MemoryStream Stream, string FileType, string FileName);

// Handler
public class ExportWifiUsersHandler : IRequestHandler<ExportWifiUsersQuery, ExportWifiUsersVm>
{
    #region props

    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly ICoursesRepository _courseRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly ICsvParser _csvParser;

    public ExportWifiUsersHandler(ICsvParser csvParser, IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository, IPeopleRepository peopleRepository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _csvParser = csvParser;
    }
    #endregion


    public async Task<ExportWifiUsersVm> Handle(ExportWifiUsersQuery request, CancellationToken ct)
    {
        Course course = await _courseRepository.GetCurrentCoursAsync(ct);

        var now = DateTimeOffset.UtcNow;
        string fileName = $"export_wifi_{now.Date.Year}{now.Date.Month}{now.Date.Day}{now.DateTime.Hour}{now.DateTime.Second}.csv";


        IEnumerable<PersonGroupCourse> pgcs = _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(course.Id, ct);
        List<WifiAccountRow> rows = new List<WifiAccountRow>(pgcs.Count());
        foreach (var pgc in pgcs)
        {
            Person p = pgc.Person;
            string password = Common.Helpers.GenerateString.RandomAlphanumeric(8);

            if (!string.IsNullOrEmpty(p.ContactMail))
            {
                var ac = new WifiAccountRow()
                {
                    Email = p.ContactMail.Split("@").FirstOrDefault(),
                    Password = password,
                };

                rows.Add(ac);
            }
        }
         var memStream = new MemoryStream();
        var streamWriter = new StreamWriter(memStream);
        await _csvParser.WriteToStreamAsync(streamWriter, rows);

        return new ExportWifiUsersVm(memStream, "text/csv", fileName);
    }
}
