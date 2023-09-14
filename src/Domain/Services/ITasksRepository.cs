namespace Domain.Services;

public interface ITasksRepository : IRepository<Entities.Tasks.Task>
{
    // Inserts the tasks if there is not taks of same type as pending or runing.
    // Returns true if inserted

    /// <summary>
    /// Inserts the tasks if there is no taks with same type with status pending or running.
    /// </summary>
    /// <param name="newTask">New task to be inserted</param>
    /// <returns><c>true</c> if new task is inserted, otherwise <c>false</c>.</returns>
    public Task<bool> AtomicInsertTaskAsync(Entities.Tasks.Task newTask);
}