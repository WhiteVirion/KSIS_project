namespace MusicClient.Models
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Nickname { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
    }
} 