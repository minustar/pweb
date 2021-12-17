namespace Minustar.Website;

public static class FormattingExtensions
{
    public static string Format(this int? i, string? unit = default, string nullness = NotApplicable)
    {
        if (i is null)
            return nullness;

        if (unit is not null)
            return $"{i:N0}\u00A0{unit}";

        return $"{i:N0}";
    }

    public static string FormatShortDate(this DateTimeOffset? dateTime, string nullness = NotApplicable)
        => dateTime == null ? nullness : $"{dateTime:MMMdd}";

    public static string FormatDate(this DateTimeOffset? dateTime, string nullness = NotApplicable)
        => dateTime == null ? nullness : $"{dateTime:dd MMMM yyyy}";

    public static string FormatAsEmoji(this bool? value,
                                       string trueEmoji = "✔️",
                                       string falseEmoji = "❌",
                                       string nullEmoji = "❓")
    {
        if (!value.HasValue)
            return nullEmoji;

        return value.Value ? trueEmoji : falseEmoji;
    }
}
