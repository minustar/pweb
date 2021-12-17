namespace Minustar.Website.Services;

public interface ICollatorService
{
    ICollator? FindCollator(string? typeName);
}
