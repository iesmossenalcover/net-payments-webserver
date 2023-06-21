namespace Application.Common.Services;

public interface ITasksRepository : IRepository<Domain.Entities.Tasks.Task>
{
    // Inserts the tasks if there is not taks of same type as pending or runing.
    // Returns true if inserted
    public Task<bool> AtomiInsertTaskAsync(Domain.Entities.Tasks.Task newTask);
}