using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Minustar.Website.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext db;

        public IndexModel(AppDbContext db)
        {
            this.db = db;
        }

        [ViewData] public string Title => LongTitle;

        public int? LanguageCount { get; private set; }

        public void OnGet()
        {
            LanguageCount = db?.Languages?.Count();
        }
    }
}
