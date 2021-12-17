namespace Minustar.Website.Models;

public class AlternateForm
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public virtual DictionaryEntry Entry { get; set; }

    [MaxLength(TitleLength)]
    public string? Trigger { get; set; }

    [Required]
    [MaxLength(TitleLength)]
    public string Headword { get; set; }

    public int? Order { get; set; }

    public string? Contents { get; set; }
}
