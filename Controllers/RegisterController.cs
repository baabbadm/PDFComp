using Microsoft.AspNetCore.Mvc;
using FileCompressor.Data;
using FileCompressor.Models;

namespace FileCompressor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegisterController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("User already exists");
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }
    }
}
