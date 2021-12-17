namespace Minustar.Website.Pages.Languages.Language
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext db;
        private readonly IAuthorizationService? auth;
        private readonly ICollatorService? collators;
        private readonly List<EntryGroupModel> groups;

        private LanguageModel? language;

        public LanguageModel Language
        {
            get => language ?? throw new InvalidOperationException();
            private set => language = value;
        }
        public IReadOnlyCollection<EntryGroupModel> Groups => this.groups;
        public bool CanEdit { get; private set; }
        [TempData] public string? Message { get; set; }

        public IndexModel(AppDbContext db, ICollatorService? collators = null, IAuthorizationService? auth = null)
        {
            this.db = db;
            this.groups = new List<EntryGroupModel>();
            this.collators = collators;
            this.auth = auth;
        }

        public async Task<IActionResult> OnGetAsync(string lang_id)
        {
            CanEdit = await CheckEditorPolicyAsync();

            var lang = await LoadLanguageAsync(lang_id);
            if (lang is null)
                return NotFound();
            Language = lang;

            await LoadEntriesAsync(lang_id);

            return Page();
        }

        private async Task<bool> CheckEditorPolicyAsync()
        {
            // TODO: Check this
#pragma warning disable CS0162
            return true;

            if (auth is null)
                return false;

            var result = await auth.AuthorizeAsync(User, Policies.Editor);
            return result.Succeeded;
#pragma warning restore
        }

        private async Task LoadEntriesAsync(string lang_id)
        {
            var lang = await db.Languages.FindAsync(lang_id);
            if (lang is null)
                return;

            var collator = collators?.FindCollator(lang.CollatorTypeName)
                ?? DefaultCollator.Instance;

            await db.Entry(lang).Collection(x => x.Entries).LoadAsync();
            var groupedEntries = new Dictionary<int, List<DictionaryEntry>>();
            foreach (var entry in lang.Entries)
            {
                // Let's load the reversals and the alt froms.
                await db.Entry(entry).Collection(x => x.AlternateForms).LoadAsync();
                await db.Entry(entry).Collection(x => x.Reversaks).LoadAsync();

                int firstLetterKey = entry.FirstLetter?.Primary ?? 0;
                if (!groupedEntries.ContainsKey(firstLetterKey))
                    groupedEntries.Add(firstLetterKey, new());
                groupedEntries[firstLetterKey].Add(entry);
            }

            var keys = groupedEntries.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
            {
                var list = groupedEntries[key];

                var firstLetters = (from e in list
                                    select e.FirstLetter)
                                   .Distinct()
                                   .ToList();

                firstLetters.Sort();
                list.Sort(collator);

                var groupModel = new EntryGroupModel(firstLetters, list);
                groups.Add(groupModel);
            }
        }

        public async Task<IActionResult> OnGetRefreshFirstLettersAsync(string lang_id)
        {
            var lang = await db.Languages.FindAsync(lang_id);
            if (lang is null)
                return NotFound();

            var collator = collators?.FindCollator(lang.CollatorTypeName)
                ?? DefaultCollator.Instance;

            var items = from e in db.Entries
                        where e.Language == lang
                        select e;

            foreach (var entry in items)
            {
                entry.FirstLetter = collator.GetFirstLetter(entry.SortKey ?? entry.Headword);
                db.Update(entry);
            }

            int changes = await db.SaveChangesAsync();

            Message = $"{changes} changes were saved.";
            return RedirectToPage(new { lang_id });
        }

        private async Task<LanguageModel?> LoadLanguageAsync(string lang_id)
        {
            var item = await db.Languages.FindAsync(lang_id);

            if (item is null)
                return null;

            var langModel = new LanguageModel(lang_id, item.Name)
            {
                NativeName = item.NativeName,
                Abstraft = item.Abstract
            }; 

            return langModel;
        }

        public record LanguageModel(string Id, string Name)
        {
            public string? NativeName { get; init; }
            public string? Abstraft { get; init; }
        }

        public record EntryGroupModel(IEnumerable<LetterInfo> Letters, IEnumerable<DictionaryEntry> Entries)
        {
        }
    }
}