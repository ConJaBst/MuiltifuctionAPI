using ConnorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConnorAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NightbotController : Controller
    {
        [HttpGet("{input}")]
        public ActionResult GetResponse(string input)
        {
            if (input.Equals("1"))
                return Ok(SayHi());
            else if (input.Equals("2"))
                return Ok(SayBye());
            else if (input.Equals("Backup"))
                return Ok(UpdateJSONAsync());
            else
                return BadRequest("Invalid input");
        }


        private static String SayHi()
        {
            return "hi";
        }
        private static String SayBye()
        {
            return "bye";
        }
        private static async Task<string> UpdateJSONAsync()
        {

            var gitHubService = new GithubService();
            await gitHubService.BackupJsonToFileAsync("wwwroot/json/galnetArticles.json");
            return ("Updating Backup");
        }
    }

    
}
