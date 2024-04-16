using Domain.Entities.People;

namespace Application.Common.Models;

public class BatchUploadModel
{
    public required IDictionary<string, Person> People { get; set; }
    public required IDictionary<string, Group> Groups { get; set; }
    public required IEnumerable<PersonGroupCourse> PersonGroupCourses { get; set; }
    public required IEnumerable<PersonGroupCourse> PersonGroupCoursesToDelete { get; set; }
    public IEnumerable<Person> NewPeople => People.Values.Where(x => x.Id == 0);
    public IEnumerable<Person> ExistingPeople => People.Values.Where(x => x.Id > 0);
    public IEnumerable<Group> NewGroups => Groups.Values.Where(x => x.Id == 0);
    public IEnumerable<Group> ExistingGroups => Groups.Values.Where(x => x.Id > 0);
    public IEnumerable<PersonGroupCourse> NewPersonGroupCourses => PersonGroupCourses.Where(x => x.Id == 0);
    public IEnumerable<PersonGroupCourse> ExistingPersonGroupCourses => PersonGroupCourses.Where(x => x.Id > 0);
}
