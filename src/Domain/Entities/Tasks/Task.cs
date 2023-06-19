namespace Domain.Entities.Tasks;

public enum TaskStatus
{
    PENDING = 0,
    RUNNING = 1,
    SUCCESS = 2,
    ERROR = 3,
}

public enum TaskType
{
    SYNC_USERS_TO_GOOGLE_WORKSPACE = 1,
}

public class Task : Entity
{
    public TaskType Type { get; set; }
    public TaskStatus Status { get; set; }
    public DateTimeOffset Start { get; set; } = default!;
    public DateTimeOffset? End { get; set; } = default!;
}