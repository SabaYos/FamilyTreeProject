using LoginAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace LoginAPI.Controllers
{
    public class DebugController : Controller
    {
        [HttpGet("generate-jwt-key")]
        public IActionResult GenerateJwtKey()
        {
            var key = RandomKeyGenerator.GenerateRandomKey();
            return Ok(new { JwtKey = key });
        }
    }
}
