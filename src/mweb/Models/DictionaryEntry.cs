namespace Minustar.Website.Models;

public class DictionaryEntry : ISortable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DictionaryLanguage Language { get; set; }

    public bool IsErrForm { get; set; }

    [Required]
    [MaxLength(TitleLength)]
    public string Headword { get; set; }

    [MaxLength(TitleLength)]
    public string? SortKey { get; set; }

    [MaxLength(ShortLength)]
    public string? Type { get; set; }

    [MaxLength(TitleLength)]
    public string? Pronunciation { get; set; }

    public virtual EntryTarget? Target { get; set; }

    public virtual LetterInfo? FirstLetter { get; set; }

    public string Contents { get; set; }

    public virtual ICollection<AlternateForm> AlternateForms { get; set; }

    public virtual ICollection<ReversalEntry> Reversaks { get; set; }

    public static string GenerateEntryTarget(DictionaryEntry entry, string prefix = "T_")
    {
        var builder = new StringBuilder(prefix);

        string key = entry?.SortKey ?? entry?.Headword ?? string.Empty;
        for (int i = 0; i < key.Length; i++)
        {
            builder.Append(key[i] switch {
                >= '0' and <= '9' => key[i],
                >= 'a' and <= 'z' => key[i],
                >= 'A' and <= 'Z' => key[i],
                _ => $".{(ushort)key[i]:X}."
            });
        }

        return builder.ToString();
    }
}
