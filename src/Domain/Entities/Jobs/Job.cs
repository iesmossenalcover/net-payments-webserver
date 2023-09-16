using Domain.Entities.Logs;

namespace Domain.Entities.Jobs;

public enum JobStatus
{
    PENDING = 0,
    RUNNING = 1,
    FINISHED = 2
}

public enum JobType
{
    MOVE_PEOPLE_GOOGLE_WORKSPACE = 1,
    SUSPEND_GOOGLE_WORKSPACE = 2,
}

public class Job : Entity
{
    public required JobType Type { get; set; }
    public required JobStatus Status { get; set; } = JobStatus.PENDING;
    public required DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public LogStoreInfo? Log { get; set; }

}