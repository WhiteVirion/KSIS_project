using Microsoft.AspNetCore.Http;
using test_server.Data; // Изменено с MusicCloud.Server.Data
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting; // Для IWebHostEnvironment
using TagLib; // Убедитесь, что TagLib используется как TagLib.File или через using static
using Dapper;
using Microsoft.AspNetCore.Hosting;
using System.Data.SQLite;
using System.Linq;

namespace test_server.Services // Изменено с MusicCloud.Server.Services
{
    public class TrackService
    {
        private readonly string _connectionString;
        private readonly string _uploadsFolderPath; // Путь к папке music_uploads
        private readonly ConnectionStringHolder _connectionStringHolder;
        private readonly IWebHostEnvironment _environment;

        // Конструктор принимает ConnectionStringHolder и IWebHostEnvironment
        public TrackService(ConnectionStringHolder connectionStringHolder, IWebHostEnvironment env)
        {
            _connectionStringHolder = connectionStringHolder;
            _environment = env;
            _connectionString = connectionStringHolder.ConnectionString;
            // Определяем путь к папке music_uploads относительно корня проекта сервера.
            // Предполагаем, что папка music_uploads находится в корне проекта сервера.
            // Если она в wwwroot, то Path.Combine(env.WebRootPath, "music_uploads");
            _uploadsFolderPath = Path.Combine(env.ContentRootPath, "music_uploads");

            // Убедимся, что основная папка для загрузок существует
            if (!Directory.Exists(_uploadsFolderPath))
            {
                Directory.CreateDirectory(_uploadsFolderPath);
            }
        }

        private SQLiteConnection GetConnection() => new SQLiteConnection(_connectionStringHolder.ConnectionString);

        // Вспомогательный метод для вычленения артиста и названия из имени файла
        private (string? artist, string title) ParseArtistAndTitleFromFileName(string fileNameToParse)
        {
            var name = Path.GetFileNameWithoutExtension(fileNameToParse);
            var parts = name.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return (parts[0].Trim(), parts[1].Trim());
            }
            return (null, name.Trim());
        }

        // Метод для загрузки трека
        public async Task<(bool Success, string Message, Track? NewTrack)> UploadTrackAsync(IFormFile file, int userId, string? titleParam = null, string? artistParam = null)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Файл не предоставлен или пуст.", null);
            }

            var originalFileName = Path.GetFileName(file.FileName);
            string fileNameOnDisk = $"{Path.GetFileNameWithoutExtension(originalFileName).Replace(" ", "_")}_{Guid.NewGuid():N}{Path.GetExtension(originalFileName)}";
            var filePath = Path.Combine(_uploadsFolderPath, fileNameOnDisk);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string? tagTitle = null;
                string? tagArtist = null;
                string? tagAlbum = null;
                double trackDuration = 0;

                try
                {
                    using (var tagFile = TagLib.File.Create(filePath))
                    {
                        tagTitle = tagFile.Tag.Title;
                        tagArtist = tagFile.Tag.FirstPerformer ?? (tagFile.Tag.Performers.Any() ? tagFile.Tag.Performers[0] : null);
                        tagAlbum = tagFile.Tag.Album;
                        trackDuration = tagFile.Properties.Duration.TotalSeconds;
                    }
                }
                catch (Exception tagEx)
                {
                    Console.WriteLine($"Ошибка чтения тегов для файла {filePath}: {tagEx.Message}");
                }

                string finalTitle = titleParam;
                string finalArtist = artistParam;
                
                if (string.IsNullOrWhiteSpace(finalTitle)) finalTitle = tagTitle;
                if (string.IsNullOrWhiteSpace(finalArtist)) finalArtist = tagArtist;

                if (string.IsNullOrWhiteSpace(finalTitle))
                {
                    var parsed = ParseArtistAndTitleFromFileName(originalFileName);
                    if (string.IsNullOrWhiteSpace(finalArtist)) finalArtist = parsed.artist;
                    finalTitle = parsed.title;
                }
                
                if (string.IsNullOrWhiteSpace(finalTitle)) finalTitle = Path.GetFileNameWithoutExtension(originalFileName);
                if (string.IsNullOrWhiteSpace(finalArtist)) finalArtist = "Unknown Artist";

                    var newTrack = new Track
                    {
                    Title = finalTitle,
                    Artist = finalArtist,
                    Album = tagAlbum ?? "", // Используем альбом из тегов, если есть
                    Duration = trackDuration, // Используем длительность из тегов
                    FilePath = filePath,
                    FileName = fileNameOnDisk, // Имя файла на диске (уникальное)
                        UserId = userId,
                    UploadedAt = DateTime.UtcNow.ToString("o") // Добавляем UploadedAt
                };

                using var connection = GetConnection();
                var sql = "INSERT INTO Tracks (Title, Artist, Album, Duration, FilePath, FileName, UserId, UploadedAt) " +
                          "VALUES (@Title, @Artist, @Album, @Duration, @FilePath, @FileName, @UserId, @UploadedAt) RETURNING Id;";
                newTrack.Id = await connection.ExecuteScalarAsync<int>(sql, newTrack);

                    return (true, "Трек успешно загружен.", newTrack);
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return (false, $"Ошибка при загрузке трека: {ex.Message}", null);
            }
        }

        // Метод для получения списка всех треков
        public async Task<IEnumerable<Track>> GetAllTracksAsync(int userId)
        {
            using var connection = GetConnection();
            var tracks = await connection.QueryAsync<Track>("SELECT Id, Title, Artist, Album, Duration, FilePath, FileName, UserId, UploadedAt FROM Tracks ORDER BY Title;");
            
            if (tracks.Any() && userId > 0)
            {
                var likedTrackIds = await connection.QueryAsync<int>("SELECT TrackId FROM UserLikedTracks WHERE UserId = @UserId", new { UserId = userId });
                var likedSet = new HashSet<int>(likedTrackIds);

                foreach (var track in tracks)
                {
                    track.IsLiked = likedSet.Contains(track.Id);
                }
            }
            return tracks;
        }

        // Метод для получения информации о треке по ID (включая путь к файлу)
        public async Task<Track?> GetTrackByIdAsync(int trackId)
        {
            using var connection = GetConnection();
            return await connection.QuerySingleOrDefaultAsync<Track>("SELECT Id, Title, Artist, Album, Duration, FilePath, FileName, UserId, UploadedAt FROM Tracks WHERE Id = @TrackId;", new { TrackId = trackId });
        }

        // Удаление трека по ID
        public async Task<(bool Success, string Message)> DeleteTrackAsync(int trackId, int userId)
        {
            using var connection = GetConnection();
            
            var track = await connection.QuerySingleOrDefaultAsync<Track>(
                "SELECT Id, FilePath, UserId FROM Tracks WHERE Id = @TrackId;", 
                new { TrackId = trackId });

            if (track == null)
            {
                return (false, "Трек не найден.");
            }

            if (track.UserId != userId)
            {
                // В будущем здесь можно добавить проверку на роль администратора
                return (false, "Доступ запрещен. Вы можете удалять только свои треки.");
            }

            try
            {
                // 1. Удалить файл с диска
                if (!string.IsNullOrEmpty(track.FilePath) && System.IO.File.Exists(track.FilePath))
                {
                    System.IO.File.Delete(track.FilePath);
                }
                else if (!string.IsNullOrEmpty(track.FilePath))
                {
                     // Файл не найден, но запись в БД есть. Можно залогировать.
                    Console.WriteLine($"Файл для трека ID {trackId} не найден по пути: {track.FilePath}, но запись будет удалена из БД.");
                }


                // 2. Удалить запись из БД
                var affectedRows = await connection.ExecuteAsync("DELETE FROM Tracks WHERE Id = @TrackId AND UserId = @UserId;", new { TrackId = trackId, UserId = userId });

                if (affectedRows > 0)
                {
                    return (true, "Трек успешно удален.");
                }
                else
                {
                    // Этого не должно произойти, если предыдущие проверки пройдены, но на всякий случай
                    return (false, "Не удалось удалить трек из базы данных или трек не найден/не принадлежит вам (повторная проверка).");
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при удалении трека ID {trackId}: {ex.Message}");
                return (false, $"Внутренняя ошибка сервера при удалении трека: {ex.Message}");
            }
        }

        // Метод для поиска треков
        public async Task<IEnumerable<Track>> SearchTracksAsync(string query, int userId)
        {
            using var connection = GetConnection();
            var searchQuery = $"%{query.ToLower()}%";
            var sql = "SELECT Id, Title, Artist, Album, Duration, FilePath, FileName, UserId, UploadedAt FROM Tracks " +
                      "WHERE /*UserId = @UserId AND*/ (LOWER(Title) LIKE @Query OR LOWER(Artist) LIKE @Query) " +
                      "ORDER BY Title;";
            var tracks = await connection.QueryAsync<Track>(sql, new { Query = searchQuery });

            if (tracks.Any() && userId > 0)
            {
                var likedTrackIds = await connection.QueryAsync<int>("SELECT TrackId FROM UserLikedTracks WHERE UserId = @UserId", new { UserId = userId });
                var likedSet = new HashSet<int>(likedTrackIds);

                foreach (var track in tracks)
                {
                    track.IsLiked = likedSet.Contains(track.Id);
                }
            }
            return tracks;
        }

        // Метод для добавления трека в понравившиеся (лайк)
        public async Task<bool> LikeTrackAsync(int userId, int trackId)
        {
            try
            {
                using var connection = GetConnection();
                // INSERT OR IGNORE добавит запись, если её нет, и ничего не сделает (без ошибки), если она уже существует.
                // Мы считаем операцию успешной, если не возникло исключений (например, из-за нарушения FOREIGN KEY).
                var sql = "INSERT OR IGNORE INTO UserLikedTracks (UserId, TrackId) VALUES (@UserId, @TrackId);";
                await connection.ExecuteAsync(sql, new { UserId = userId, TrackId = trackId });
                return true; // Успех, если не было исключений (трек теперь лайкнут или уже был лайкнут)
            }
            catch (Exception ex)
                {
                // Это может произойти, если, например, trackId или userId не существуют и есть FOREIGN KEY constraint
                Console.WriteLine($"Ошибка при добавлении лайка для UserId {userId}, TrackId {trackId}: {ex.Message}");
                return false;
            }
        }

        // Метод для удаления трека из понравившихся (анлайк)
        public async Task<bool> UnlikeTrackAsync(int userId, int trackId)
                    {
            try
            {
                using var connection = GetConnection();
                var sql = "DELETE FROM UserLikedTracks WHERE UserId = @UserId AND TrackId = @TrackId;";
                var affectedRows = await connection.ExecuteAsync(sql, new { UserId = userId, TrackId = trackId });
                return affectedRows > 0; // true, если лайк был и его удалили; false, если лайка не было
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении лайка для UserId {userId}, TrackId {trackId}: {ex.Message}");
                return false;
            }
        }

        // Новый метод для получения списка понравившихся треков пользователя
        public async Task<IEnumerable<Track>> GetUserLikedTracksAsync(int userId)
        {
            using var connection = GetConnection();
            var sql = @"
                SELECT T.Id, T.Title, T.Artist, T.Album, T.Duration, T.FilePath, T.FileName, T.UserId, T.UploadedAt
                FROM Tracks T
                INNER JOIN UserLikedTracks ULT ON T.Id = ULT.TrackId
                WHERE ULT.UserId = @UserId
                ORDER BY T.Title;";
            var tracks = await connection.QueryAsync<Track>(sql, new { UserId = userId });
            foreach (var track in tracks)
            {
                track.IsLiked = true; // Все треки здесь по определению лайкнуты
            }
            return tracks;
        }
    }
} 