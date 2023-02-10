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
                        .Where(x => x.Course.Active == true && peopleIds.Distinct().Contains(x.PersonId)).ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetGroupsByNameAsync(IEnumerable<string> names, CancellationToken ct)
    {
        return await _dbContext.Groups.Where(x => names.Distinct().Contains(x.Name)).ToListAsync(ct);
    }

    public async Task<IEnumerable<Person>> GetPeopleAsync(IEnumerable<string> documents, CancellationToken ct)
    {
        return await _dbContext.People.Where(x => documents.Distinct().Contains(x.DocumentId)).ToListAsync(ct);
    }

    public async Task<IEnumerable<Student>> GetStudentsByAcademicRecordAsync(IEnumerable<long> academicRecords, CancellationToken ct)
    {
        return await _dbContext.Students
            .Where(x => academicRecords.Distinct().Contains(x.AcademicRecordNumber))
            .ToListAsync(ct);
    }

    public async Task InsertAndUpdateTransactionAsync(
        Application.Common.Models.BatchUploadModel batchUploadModel,
        CancellationToken ct)
    {
        _dbContext.Groups.AddRange(batchUploadModel.NewGroups);
        _dbContext.Groups.UpdateRange(batchUploadModel.ExistingGroups);
        _dbContext.People.AddRange(batchUploadModel.NewPeople);
        _dbContext.People.UpdateRange(batchUploadModel.ExistingPeople);
        _dbContext.PersonGroupCourses.AddRange(batchUploadModel.NewPersonGroupCourses);
        _dbContext.PersonGroupCourses.UpdateRange(batchUploadModel.ExistingPersonGroupCourses);
        await _dbContext.SaveChangesAsync();
    }

    public async Task InsertPersonAsync(Person person)
    {
        _dbContext.People.Add(person);
        await _dbContext.SaveChangesAsync();
    }

    public async Task InsertStudentAsync(Student student)
    {
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Student?> GetStudentByAcademicRecordAsync(long academicRecordNumber, CancellationToken ct)
    {
        return await _dbContext.Students.FirstOrDefaultAsync(x => x.AcademicRecordNumber == academicRecordNumber);
    }

    public async Task<Person?> GetPersonByDocumentIdAsync(string documentId, CancellationToken ct)
    {
        return await _dbContext.People.FirstOrDefaultAsync(x => x.DocumentId == documentId);
    }
}