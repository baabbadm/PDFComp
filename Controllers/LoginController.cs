using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace FileCompressor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult Login([FromForm] string email, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BadRequest("Email and password are required.");

            string passwordHash = ComputeSha256Hash(password);
            string connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @e AND password_hash = @p", connection);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@p", passwordHash);

            var result = Convert.ToInt32(cmd.ExecuteScalar());

            if (result > 0)
            {
                // ✅ تسجيل الدخول ناجح — نرسل Session ID أو Cookie أو Token حسب الحاجة
                HttpContext.Session.SetString("user", email);
                return Ok("Login successful");
            }
            else
            {
                return Unauthorized("Invalid email or password");
            }
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
