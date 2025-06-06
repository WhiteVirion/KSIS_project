namespace MusicCloud.Server.Data
{
    public class Track
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Кто загрузил трек
        public string Title { get; set; }
        public string? Artist { get; set; } // Nullable
        public string FileName { get; set; }
        public string FilePath { get; set; } // Путь к файлу на сервере
        public string UploadedAt { get; set; } // По ТЗ строка, но DateTime был бы лучше
    }
} 