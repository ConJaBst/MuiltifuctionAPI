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
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            var articles = await _galnetService.LoadArticlesFromJsonAsync();

            // Default sorting by ID descending
            articles = sortOrder switch
            {
                "id" => articles.OrderBy(a => a.Id).ToList(),
                "title" => articles.OrderBy(a => a.Title).ToList(),
                "title_desc" => articles.OrderByDescending(a => a.Title).ToList(),
                "date" => articles.OrderBy(a => a.Date).ToList(),
                "date_desc" => articles.OrderByDescending(a => a.Date).ToList(),
                _ => articles.OrderByDescending(a => a.Id).ToList(), // Default to Id descending
            };

            ViewData["IdSortParam"] = sortOrder == "id" ? "id_desc" : "id";
            ViewData["TitleSortParam"] = sortOrder == "title" ? "title_desc" : "title";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";

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
