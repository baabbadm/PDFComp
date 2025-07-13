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

            var cmd = new MySqlCommand(@"
                SELECT code, created_at 
                FROM verification_codes 
                WHERE email = @e 
                ORDER BY created_at DESC 
                LIMIT 1", connection);
            cmd.Parameters.AddWithValue("@e", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var storedCode = reader["code"].ToString();
                var createdAt = Convert.ToDateTime(reader["created_at"]);

                // تحقق من مدة صلاحية الكود (10 دقائق مثلاً)
                if ((DateTime.UtcNow - createdAt).TotalMinutes > 10)
                    return Unauthorized("Verification code expired.");

                if (storedCode == code)
                {
                    reader.Close(); // ضروري قبل تنفيذ أمر جديد على نفس الاتصال

                    // حذف الكود بعد التحقق (اختياري)
                    var deleteCmd = new MySqlCommand("DELETE FROM verification_codes WHERE email = @e", connection);
                    deleteCmd.Parameters.AddWithValue("@e", email);
                    deleteCmd.ExecuteNonQuery();

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
