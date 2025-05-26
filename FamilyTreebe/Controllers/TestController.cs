using LoginAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public TestController(IOptions<PasswordHasherOptions> options)
        {
            _passwordHasher = new PasswordHasher<ApplicationUser>(options);
        }

        [HttpGet("generatehash")]
        public IActionResult GenerateHash(string password)
        {
            var hash = _passwordHasher.HashPassword(null, password);
            return Ok(new { PasswordHash = hash });
        }
    }
}
