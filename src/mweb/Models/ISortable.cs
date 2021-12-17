namespace Minustar.Website.Models;

public interface ISortable
{
    string Headword { get; }
    string? SortKey { get; }
}
