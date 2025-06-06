namespace test_server.Data
{
    public class Track
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string? Artist { get; set; } // Знак ? важен
        public string? Album { get; set; } // Добавлено свойство Album
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedAt { get; set; }
        public double Duration { get; set; } // Длительность в секундах
        public bool IsLiked { get; set; } // Добавлено для отслеживания лайков пользователя
    }
} 