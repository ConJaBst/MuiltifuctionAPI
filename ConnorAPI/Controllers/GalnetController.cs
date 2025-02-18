using ConnorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConnorAPI.Controllers
{
    public class GalnetController : Controller
    {
        private readonly GalnetService _galnetService;

        public GalnetController(GalnetService galnetService)
        {
            _galnetService = galnetService;
        }

        public async Task<IActionResult> Update()
        {
            await _galnetService.UpdateJsonAsync();

            Console.WriteLine("Backing up JSON");
            var gitHubService = new GithubService();
            await gitHubService.BackupJSONToGitHub("wwwroot/json/galnetArticles.json");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index(string sortOrder, string searchQuery = null, bool searchTitle = false, bool searchContent = false, bool loadAll = false)
        {
            var articles = await _galnetService.LoadArticlesFromJsonAsync();

            articles = articles.OrderByDescending(a => a.Id).ToList();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                if (searchTitle && searchContent)
                {
                    articles = articles.Where(a =>
                            (a.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                            (a.Content.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                        ).ToList();
                    loadAll = true;
                }
                else if (searchTitle && !searchContent)
                {
                    articles = articles.Where(a =>
                            (a.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) 
                        ).ToList();
                    loadAll = true;

                }
                else if (!searchTitle && searchContent)
                {
                    articles = articles.Where(a =>
                            (a.Content.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                        ).ToList();
                    loadAll = true;

                }

                else
                {
                    Console.WriteLine("No search criteria selected");
                }
            }

            if (!loadAll)
            {
                articles = articles.Take(10).ToList(); // Load only 15
            }

            // Sorting code
            articles = sortOrder switch
            {
                "id" => articles.OrderBy(a => a.Id).ToList(),
                "title" => articles.OrderBy(a => a.Title).ToList(),
                "title_desc" => articles.OrderByDescending(a => a.Title).ToList(),
                "date" => articles.OrderBy(a => a.Date).ToList(),
                "date_desc" => articles.OrderByDescending(a => a.Date).ToList(),
                _ => articles // Keep the default order (by descending Id) if no other sort is specified
            };

            ViewData["IdSortParam"] = sortOrder == "id" ? "id_desc" : "id";
            ViewData["TitleSortParam"] = sortOrder == "title" ? "title_desc" : "title";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["LoadAll"] = loadAll;

            return View(articles);
        }



        public async Task<IActionResult> ViewContent(int id)
        {
            var articles = await _galnetService.LoadArticlesFromJsonAsync();
            var article = articles.FirstOrDefault(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article); // Pass only a single article, not a list
        }
    }
}
