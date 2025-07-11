namespace FileCompressor.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int UploadCount { get; set; } = 0;
    }
}