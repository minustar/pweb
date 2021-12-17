namespace Minustar.Website.Pages.Languages;

public class IndexModel : PageModel
{
    private readonly AppDbContext db;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IAuthorizationService auth;

    [ViewData]
    public string Title => MakeTitle("Languages");

    public List<LanguageIndexViewModel> Items { get; }
    public bool CanEdit { get;private set; }

    public IndexModel(AppDbContext db, IAuthorizationService auth, SignInManager<IdentityUser> signInManager)
    {
        this.db = db;
        this.auth = auth;
        this.signInManager = signInManager;

        Items = new List<LanguageIndexViewModel>();
    }

    public async Task OnGetAsync()
    {
        var query = from lang in db.Languages.Include(x => x.Entries)
                    orderby lang.Name
                    select new LanguageIndexViewModel(
                        lang.Id,
                        lang.Name,
                        lang.NativeName,
                        lang.Abstract,
                        lang.Entries.Count);

        Items.AddRange(query);

        CanEdit = await CheckCanEditAsync();
    }

    private async Task<bool> CheckCanEditAsync()
    {
        if (signInManager.IsSignedIn(User))
        {
            var result = await auth.AuthorizeAsync(User, Policies.Editor);
            return result.Succeeded;
        }

        return false;
    }

    public record LanguageIndexViewModel (
        string Id,
        string Name,
        string? NativeName,
        string? Abstract,
        int EntryCount
        );
}
