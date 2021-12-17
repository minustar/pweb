namespace Minustar.Website.Models;

public class DictionaryLanguage
{
    [Key]
    [Required]
    [MaxLength(ShortLength)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; }

    [Required]
    [MaxLength(TitleLength)]
    public string Name { get; set; }

    [MaxLength(TitleLength)]
    public string? NativeName { get; set; }

    public string? Abstract { get; set; }

    [MaxLength(255)]
    public string? CollatorTypeName { get; set; }

    public virtual ICollection<DictionaryEntry> Entries { get; set; }
    public virtual ICollection<Abbreviation> Abbreviations { get; set; }
}
