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
    public required TaskType Type { get; set; }
    public required TaskStatus Status { get; set; }
    public required DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
}