using Application.Common.Services;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class PeopleService : IPeopleService
{
    #region properties

        private readonly ApplicationDbContext _dbContext;

        public PeopleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    #endregion

    public Task<Course> GetCurrentCoursAsync(CancellationToken ct)
    {
        return _dbContext
            .Courses
            .FirstAsync(x => x.Active == true);
    }

    public async Task<IEnumerable<PersonGroupCourse>> GetCurrentCoursePersonGroupByPeopleIdsAsync(IEnumerable<long> peopleIds, CancellationToken ct)
    {
        return await _dbContext.PersonGroupCourses
                        .Include(x => x.Person)
                        .Include(x => x.Group)
                        .Include(x => x.Course)
                        .Where(x => x.Course.Active == true && peopleIds.Contains(x.PersonId)).ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetGroupsByNameAsync(IEnumerable<string> names, CancellationToken ct)
    {
        return await _dbContext.Groups.Where(x => names.Contains(x.Name)).ToListAsync(ct);
    }

    public async Task<IEnumerable<Person>> GetPeopleAsync(IEnumerable<string> documents, CancellationToken ct)
    {
        return await _dbContext.People.Where(x => documents.Contains(x.DocumentId)).ToListAsync(ct);
    }

    public async Task<IEnumerable<Student>> GetStudentsByAcademicRecordAsync(IEnumerable<long> expidients, CancellationToken ct)
    {
        var query = _dbContext
                    .Students
                    .Include(x => x.Person)
                    .Where(x => expidients.Contains(x.AcademicRecordNumber));

        return await query.ToListAsync();
    }

    public async Task InsertAndUpdateTransactionAsync(
        Application.Common.Models.BatchUploadModel batchUploadModel,
        CancellationToken ct)
    {
        _dbContext.Groups.AddRange(batchUploadModel.NewGroups);
        _dbContext.Groups.UpdateRange(batchUploadModel.ExistingGroups);
        _dbContext.People.AddRange(batchUploadModel.NewPeople);
        _dbContext.People.UpdateRange(batchUploadModel.ExistingPeople);
        _dbContext.Students.AddRange(batchUploadModel.NewStudents);
        _dbContext.Students.UpdateRange(batchUploadModel.ExistingStudents);
        _dbContext.PersonGroupCourses.AddRange(batchUploadModel.NewPersonGroupCourses);
        _dbContext.PersonGroupCourses.UpdateRange(batchUploadModel.ExistingPersonGroupCourses);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Student> InsertStudentAsync(Student student)
    {
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();
        return student;
    }
}