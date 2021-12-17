namespace Minustar.Website.Pages.Languages.Language;

public class NewModel : PageModel
{
    private readonly AppDbContext db;
    private readonly ICollatorService? collators;
    private readonly List<WordTypeModel> wordTypes;

    private LanguageModel? language;

    public NewModel(AppDbContext db, ICollatorService? collators = null)
    {
        this.db = db;
        this.collators = collators;
        this.wordTypes = new List<WordTypeModel>();
    }

    [BindProperty]
    public NewWordInputModel Input { get; set; }

    public ICollection<WordTypeModel> WordTypes { get => wordTypes; }

    public LanguageModel Language
    {
        get => language ?? throw new InvalidOperationException();
        private set => language = value;
    }

    public async Task<IActionResult> OnGetAsync(string lang_id)
    {
        var language = await GetLanguageDataAsync(lang_id);
        if (language is null)
            return NotFound();

        Language = language;

        await LoadWordTypesAsync(lang_id);

        return Page();
    }

    private async Task LoadWordTypesAsync(string lang_id)
    {
        var lang = await db.Languages.FindAsync(lang_id);
        var abbrev = (from a in db.Abbreviations
                      where a.Language == lang
                      where a.Kind == AbbreviationKind.WordType
                      select new WordTypeModel(
                          a.Key,
                          a.Value
                      )).ToList();

        var usedWordTypes = (from e in db.Entries
                             where e.Language == lang
                             where e.Type != null
                             select e.Type)
                            .ToList()
                            .Except(abbrev.Select(x => x.Key))
                            .Distinct()
                            .Select(x => new WordTypeModel(x));


        abbrev.AddRange(usedWordTypes);
        abbrev.Sort((a, b) => String.Compare(a.Key, b.Key));

        wordTypes.AddRange(abbrev);
    }

    public async Task<IActionResult> OnPostAsync(string lang_id)
    {
        if (ModelState.IsValid)
        {
            var lang = await db.Languages.FindAsync(lang_id);
            var e = new DictionaryEntry
            {
                Language = lang,

                IsErrForm = Input.IsErrForm,
                Headword = Input.Headword,
                SortKey = Input.SortKey,
                Pronunciation = Input.Pronunciation,
                Type = Input.WordType,
                Contents = Input.Contents
            };

            await db.AddAsync(e);
            await db.SaveChangesAsync();

            var collator = collators?.FindCollator(lang.CollatorTypeName)
                ?? DefaultCollator.Instance;

            var fl = collator.GetFirstLetter(e.SortKey ?? e.Headword);
            e.FirstLetter = fl;

            db.Update(e);
            await db.SaveChangesAsync();

            return RedirectToPage("./Index", new { lang_id });
        }

        return await OnGetAsync(lang_id);
    }

    private async Task<LanguageModel?> GetLanguageDataAsync(string lang_id)
    {
        var item = await db.Languages.FindAsync(lang_id);
        if (item is not null)
            return new(item.Name, item.NativeName);
        return null;
    }

    public record LanguageModel(string Name, string? NativeName);
    public record CollatorItemModel(string Id, string DisplayName);

    public class NewWordInputModel
    {
        public bool IsErrForm { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = ValidationMessages.FieldIsRequired)]
        [MaxLength(TitleLength, ErrorMessage = "The file {0} must be at most {1} characters long.")]
        [Display(Name = "Headword")]
        [Description("The lemma of an etry, i.e. under what this entry will be listed.")]
        public string Headword { get; set; }

        [Display(Name = "Sort Key")]
        [MaxLength(TitleLength, ErrorMessage = "The file {0} must be at most {1} characters long.")]
        [Description("If this entry should be sorted differently than by its lemma, the actual sort key should be provided here.")]
        public string? SortKey { get; set; }

        [Display(Name = "Type")]
        [MaxLength(ShortLength, ErrorMessage = "The file {0} must be at most {1} characters long.")]
        [Description("The type of word, i.e. the part of speech this entry belongs to.")]
        public string? WordType { get; set; }

        [MaxLength(TitleLength, ErrorMessage = "The file {0} must be at most {1} characters long.")]
        [Description("The IPA transcription of the entry as it is commonly realised.")]
        public string? Pronunciation { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = ValidationMessages.FieldIsRequired)]
        [Description("The description of the current entry.")]
        public string Contents { get; set; }

        public NewWordInputModel()
        {
            Headword = string.Empty;
            Contents = string.Empty;
        }
    }

    public record WordTypeModel(string Key, string? Value = null);
}
