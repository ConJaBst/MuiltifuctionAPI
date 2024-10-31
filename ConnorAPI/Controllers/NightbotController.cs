using ConnorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConnorAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NightbotController : Controller
    {
        [HttpGet("{input}")]
        public async Task<ActionResult> GetResponseAsync(string input)
        {
            if (input.Equals("1"))
                return Ok(SayHi());
            else if (input.Equals("2"))
                return Ok(SayBye());
            else if (input.Equals("Backup")) {
                await UpdateJSONAsync();
                return Ok("Backed Up");
            }
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
            Console.WriteLine("Backing up JSON");
            var gitHubService = new GithubService();
            await gitHubService.BackupJsonToFileAsync("wwwroot/json/galnetArticles.json");
            return ("Updating Backup");
        }
    }

    
}
