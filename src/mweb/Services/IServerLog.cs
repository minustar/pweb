namespace Minustar.Website.Services;

public interface IServerLog
{
    void Log<TPayload>(TPayload? payload = default);
}

internal class DbContextServerLog<T> : IServerLog
    where T : DbContext
{
    private readonly T db;

    public DbContextServerLog(T db)
    {
        this.db = db;
    }

    public void Log<TPayload>(TPayload? payload = default)
    {
        var (category, eventType) = DbContextServerLog<T>.GetCategory<TPayload>();

        var logEntry = new ServerEvent
        {
            Category = category,
            EventType = eventType,
            Timestamp = DateTimeOffset.UtcNow,
            Payload = JsonSerializer.Serialize(payload)
        };

        db.Add(logEntry);
        db.SaveChanges();
    }

    private static (string Category, string Event) GetCategory<T>()
    {
        string? category = null, eventType = null;

        // Does the type have a category attrib?
        var type = typeof(T);
        var attrib = type.GetCustomAttribute<CategoryAttribute>();
        if (attrib?.Category is string s)
            category = s;

        return (
            category ?? string.Empty,
            eventType ?? string.Empty
            );
    }
}
