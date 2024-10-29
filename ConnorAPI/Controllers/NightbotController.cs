using Microsoft.AspNetCore.Mvc;

namespace ConnorAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NightbotController : Controller
    {
        [HttpGet("{input}")]
        public ActionResult GetResponse(int input)
        {
            if (input == 1)
                return Ok("hi");
            else if (input == 2)
                return Ok("bye");
            else
                return BadRequest("Invalid input");
        }
    }
}
