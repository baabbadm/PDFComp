using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;

namespace FileCompressor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifyController : ControllerBase
    {
        private readonly IConfiguration _config;

        public VerifyController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult Verify([FromForm] string email, [FromForm] string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                return BadRequest("Email and code are required.");

            string connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var cmd = new MySqlCommand("SELECT code FROM verification_codes WHERE email = @e ORDER BY created_at DESC LIMIT 1", connection);
            cmd.Parameters.AddWithValue("@e", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var storedCode = reader.GetString("code");
                if (storedCode == code)
                {
                    return Ok("Verification successful.");
                }
                else
                {
                    return Unauthorized("Invalid code.");
                }
            }
            else
            {
                return NotFound("Verification code not found.");
            }
        }
    }
}
