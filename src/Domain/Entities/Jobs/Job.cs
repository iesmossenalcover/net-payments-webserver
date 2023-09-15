namespace Domain.Entities.Jobs;

public enum JobStatus
{
    PENDING = 0,
    RUNNING = 1,
    SUCCESS = 2,
    ERROR = 3,
}

public enum JobType
{
    MOVE_PEOPLE_GOOGLE_WORKSPACE = 1,
}

public class Job : Entity
{
    public required JobType Type { get; set; }
    public required JobStatus Status { get; set; }
    public required DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public LogStoreInfo? Log { get; set; }

}