namespace test_server.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? AuthToken { get; set; } // Знак ? важен для Nullable, если включены nullable reference types
    }
} 