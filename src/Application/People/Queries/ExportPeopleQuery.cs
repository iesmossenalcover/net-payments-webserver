using Application.Common.Models;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

public record ExportPeopleQuery() : IRequest<FileVm>;

public class ExportPeopleQueryQuueryHandler : IRequestHandler<ExportPeopleQuery, FileVm>
{
    # region IOC
    private readonly ICoursesRepository _courseRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly ICsvParser _csvParser;

    public ExportPeopleQueryQuueryHandler(ICoursesRepository courseRepository, IPersonGroupCourseRepository personGroupCourseRepository, ICsvParser csvParser)
    {
        _courseRepository = courseRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _csvParser = csvParser;
    }

    #endregion

    public async Task<FileVm> Handle(ExportPeopleQuery request, CancellationToken ct)
    {
        Course course = await _courseRepository.GetCurrentCoursAsync(ct);
        IQueryable<PersonGroupCourse> personGroupCourses = _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(course.Id, ct);

        // Same format as the batch upload, so the exported file can be edited and uploaded again.
        IEnumerable<BatchUploadRow> rows = personGroupCourses.Select(x => new BatchUploadRow()
        {
            AcademicRecordNumber = x.Person.AcademicRecordNumber,
            DocumentId = x.Person.DocumentId,
            FirstName = x.Person.Name,
            Surname1 = x.Person.Surname1,
            Surname2 = x.Person.Surname2,
            ContactPhone = x.Person.ContactPhone,
            GroupName = x.Group.Name,
            Subjects = x.SubjectsInfo,
            Email = x.Person.ContactMail,
            Enrolled = x.Enrolled,
            IsAmipa = x.Amipa,
            SchoolAlert = x.Person.SchoolAlert,
            WalkingAuthorization = x.WalkingAuthorization,
            TransportAuthorization = x.TransportAuthorization,
        }).ToList();
        
        var memStream = new MemoryStream();
        var streamWriter = new StreamWriter(memStream);
        await _csvParser.WriteToStreamAsync(streamWriter, rows);
        return new FileVm(memStream, "text/csv", "users.csv");
    }
}
