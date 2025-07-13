using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp; 
using MimeKit;
namespace FileCompressor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RegisterController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult Register([FromForm] string email, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BadRequest("Email and password are required.");

            string passwordHash = ComputeSha256Hash(password);

            string connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var cmd = new MySqlCommand("INSERT INTO users (email, password_hash) VALUES (@email, @password_hash)", connection);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@password_hash", passwordHash);

            try
            {
                cmd.ExecuteNonQuery();
                return Ok("User registered successfully.");
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
            {
                return Conflict("Email already registered.");
            }
            catch
            {
                return StatusCode(500, "Registration failed.");
            }
        }
private void SendVerificationEmail(string toEmail, string code)
{
    var message = new MimeMessage();
    message.From.Add(MailboxAddress.Parse("rpahelpus@gmail.com"));
    message.To.Add(MailboxAddress.Parse(toEmail));
    message.Subject = "Verification Code";
    message.Body = new TextPart("plain") { Text = $"Your verification code is: {code}" };

    using var smtp = new SmtpClient();
    smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
    smtp.Authenticate("rpahelpus@gmail.com", "كلمة_مرور_التطبيق"); // تحتاج توليد App Password
    smtp.Send(message);
    smtp.Disconnect(true);
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
