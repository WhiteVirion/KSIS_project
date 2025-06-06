using Microsoft.Data.Sqlite;
using System.IO;

namespace MusicCloud.Server.Data
{
    public class DatabaseSetup
    {
        private readonly string _connectionString;

        // ConnectionStringHolder будет внедрен через DI из Program.cs
        public DatabaseSetup(ConnectionStringHolder connectionStringHolder)
        {
            _connectionString = connectionStringHolder.ConnectionString;
        }

        public void Initialize()
        {
            // Убедимся, что директория для файла БД существует, если путь содержит директории
            var dbPath = new SqliteConnectionStringBuilder(_connectionString).DataSource;
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Создание таблицы Users
                var createUserTableCommand = connection.CreateCommand();
                createUserTableCommand.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE NOT NULL,
                        PasswordHash TEXT NOT NULL,
                        AuthToken TEXT NULLABLE
                    );
                ";
                createUserTableCommand.ExecuteNonQuery();

                // Создание таблицы Tracks
                var createTracksTableCommand = connection.CreateCommand();
                createTracksTableCommand.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Tracks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        Title TEXT NOT NULL,
                        Artist TEXT NULLABLE,
                        FileName TEXT NOT NULL,
                        FilePath TEXT NOT NULL,
                        UploadedAt TEXT NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );
                ";
                createTracksTableCommand.ExecuteNonQuery();
            }
        }
    }
} 