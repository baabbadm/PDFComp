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

            var cmd = new MySqlCommand("INSERT INTO users (email, password_hash) VALUES (@e, @p)", connection);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@p", passwordHash);

            try
            {
                cmd.ExecuteNonQuery();

                // Generate and store verification code
                string code = new Random().Next(100000, 999999).ToString();
                var codeCmd = new MySqlCommand("INSERT INTO verification_codes (email, code) VALUES (@e, @c)", connection);
                codeCmd.Parameters.AddWithValue("@e", email);
                codeCmd.Parameters.AddWithValue("@c", code);
                codeCmd.ExecuteNonQuery();

                // Send verification email
                SendVerificationEmail(email, code);

                return Ok("User registered successfully. Verification code sent.");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return Conflict("Email already registered.");
            }
            catch
            {
                return StatusCode(500, "Registration failed.");
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

        private void SendVerificationEmail(string recipientEmail, string code)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("rpahelpus@gmail.com"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Verification Code";

            message.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {code}"
            };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate("rpahelpus@gmail.com", "ltkinojiouvkwthj");
            smtp.Send(message);
            smtp.Disconnect(true);
        }
    }
}
