namespace Minustar.Website.Models;

public class Abbreviation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public virtual DictionaryLanguage Language { get; set; }

    [Required]
    [MaxLength(ShortLength)]
    public AbbreviationKind Kind { get; set; }

    [Required]
    [MaxLength(ShortLength)]
    public string Key { get; set; }

    [Required]
    [MaxLength(TitleLength)]
    public string Value { get; set; }
  
    public string? Contents { get; set; }
}
