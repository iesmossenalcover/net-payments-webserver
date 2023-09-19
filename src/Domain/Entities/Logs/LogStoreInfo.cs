namespace Domain.Entities.Logs;

public enum StoreType
{
    INTO_INFO,
}

public class LogStoreInfo : Entity
{
    public required StoreType Type { get; set; }
    public required string Info { get; set; }
}