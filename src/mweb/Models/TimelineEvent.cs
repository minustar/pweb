namespace Minustar.Website.Models;

public class TimelineEvent
    : IComparable<TimelineEvent>,
      IEquatable<TimelineEvent>
{
    public int Id { get; set; }

    public bool IsApprox { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }

    public string Title { get; set; }
    public string Contents { get; set; }

    public bool Hidden { get; set; }

    public TimelineEvent()
    {
        Title = string.Empty;
        Contents = string.Empty;
    }

    public int CompareTo(TimelineEvent? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        int order = this.Year - other.Year;
        if (order == 0)
        {
            // IsApprox sorts down.
            int ax = this.IsApprox ? 1 : 0;
            int ay = other.IsApprox ? 1 : 0;
            order = ay - ax;

            if (order == 0)
            {
                ax = this.Month ?? -1;
                ay = other.Month ?? -1;
                order = ax - ay;

                if (order == 0)
                {
                    ax = this.Day ?? -1;
                    ay = other.Day ?? -1;
                    order = ax - ay;
                }
            }
        }

        if (order == 0)
            order = String.Compare(this.Title, other.Title, false);

        return order;
    }

    public override bool Equals(object? obj)
    {
        return obj is TimelineEvent @event &&
               Equals(@event);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id,
                                IsApprox,
                                Year,
                                Month,
                                Day,
                                Title,
                                Contents,
                                Hidden);
    }

    public bool Equals(TimelineEvent? other) 
    {
        if (other is not TimelineEvent)
            return false;

        return Id == other.Id &&
               IsApprox == other.IsApprox &&
               Year == other.Year &&
               Month == other.Month &&
               Day == other.Day &&
               Title == other.Title &&
               Contents == other.Contents &&
               Hidden == other.Hidden;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendFormat($"[{0}] ", Id);
        if (IsApprox) sb.Append("ca. ");
        sb.Append(Year);
        if (Month.HasValue && Day.HasValue)
        {
            sb.Append("-{0}-{1:00}", Month.Value, Day.Value);
        }

        sb.Append("\u00A0");
        sb.Append(Title);

        return sb.ToString();
    }
}
