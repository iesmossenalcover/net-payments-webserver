﻿using Domain.Entities.People;

namespace Application.Common.Models;

public class BatchUploadModel
{
    public IDictionary<string, Person> People { get; set; }
    public IDictionary<string, Group> Groups{ get; set; }
    public IEnumerable<PersonGroupCourse> PersonGroupCourses { get; set; }
    public IEnumerable<Person> NewPeople => People.Values.Where(x => x.Id == 0);
    public IEnumerable<Person> ExistingPeople => People.Values.Where(x => x.Id > 0);
    public IEnumerable<Group> NewGroups => Groups.Values.Where(x => x.Id == 0);
    public IEnumerable<Group> ExistingGroups => Groups.Values.Where(x => x.Id > 0);
    public IEnumerable<PersonGroupCourse> NewPersonGroupCourses => PersonGroupCourses.Where(x => x.Id == 0);
    public IEnumerable<PersonGroupCourse> ExistingPersonGroupCourses => PersonGroupCourses.Where(x => x.Id > 0);

    public BatchUploadModel(IDictionary<string, Person> people, IDictionary<string, Group> groups, IEnumerable<PersonGroupCourse> personGroupCourses)
    {
        People = people;
        Groups = groups;
        PersonGroupCourses = personGroupCourses;
    }
}
