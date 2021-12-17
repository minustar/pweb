namespace Minustar.Website.Models;

public class ReversalEntry : ISortable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public virtual DictionaryEntry Entry { get; set; }

    [Required]
    [MaxLength(TitleLength)]
    public string Headword { get; set; }

    [MaxLength(TitleLength)]
    public string? SortKey { get; set; }

    public string? Contents { get; set; }
}
