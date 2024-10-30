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

        public IActionResult Index()
        {
            return View();
        }
    }

}
