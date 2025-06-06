namespace MusicCloud.Server.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string? AuthToken { get; set; } // Nullable, как указано в ТЗ
    }
} 