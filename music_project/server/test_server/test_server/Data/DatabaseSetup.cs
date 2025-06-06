using System.Data.SQLite;
using System.IO;
using System;

namespace test_server.Data
{
    public class DatabaseSetup
    {
        private readonly string _connectionString;

        public DatabaseSetup(ConnectionStringHolder connectionStringHolder)
        {
            _connectionString = connectionStringHolder.ConnectionString;
        }

        public void Initialize()
        {
            try
            {
                var dbPath = new SQLiteConnectionStringBuilder(_connectionString).DataSource;
                if (string.IsNullOrEmpty(dbPath))
                {
                    Console.WriteLine("[DatabaseSetup] Error: DBPath is null or empty in connection string.");
                    return;
                }
                Console.WriteLine($"[DatabaseSetup] DBPath from connection string: {dbPath}");
                var dbDirectory = Path.GetDirectoryName(dbPath);

                // Если путь к БД относительный, делаем его абсолютным относительно директории приложения
                if (!Path.IsPathRooted(dbPath))
                {
                    dbPath = Path.Combine(AppContext.BaseDirectory, dbPath);
                    Console.WriteLine($"[DatabaseSetup] Absolute DBPath: {dbPath}");
                    // Обновляем директорию, если путь был относительным
                    dbDirectory = Path.GetDirectoryName(dbPath);
                }

                if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
                {
                    Console.WriteLine($"[DatabaseSetup] Creating directory: {dbDirectory}");
                    Directory.CreateDirectory(dbDirectory);
                }

                Console.WriteLine($"[DatabaseSetup] Using connection string: {_connectionString} (effective path: {dbPath})");
                using (var connection = new SQLiteConnection(_connectionString)) // Строка подключения остается прежней, SQLite обрабатывает путь
                {
                    connection.Open();
                    Console.WriteLine("[DatabaseSetup] DB Connection Opened.");

                    var createUserTableCommand = connection.CreateCommand();
                    createUserTableCommand.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nickname TEXT UNIQUE NOT NULL,
                            Email TEXT UNIQUE NOT NULL,
                            PasswordHash TEXT NOT NULL,
                            AuthToken TEXT NULLABLE
                        );
                    ";
                    createUserTableCommand.ExecuteNonQuery();
                    Console.WriteLine("[DatabaseSetup] Users table ensured.");

                    var createTracksTableCommand = connection.CreateCommand();
                    createTracksTableCommand.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS Tracks (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            UserId INTEGER NOT NULL,
                            Title TEXT NOT NULL,
                            Artist TEXT NULLABLE,
                            Album TEXT NULLABLE,
                            Genre TEXT NULLABLE,
                            FileName TEXT NOT NULL,
                            FilePath TEXT NOT NULL,
                            UploadedAt TEXT NOT NULL,
                            Duration REAL NOT NULL DEFAULT 0, 
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                        );
                    ";
                    createTracksTableCommand.ExecuteNonQuery();
                    Console.WriteLine("[DatabaseSetup] Tracks table ensured.");

                    // Создание таблицы UserLikedTracks
                    var createUserLikedTracksTableCommand = connection.CreateCommand();
                    createUserLikedTracksTableCommand.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS UserLikedTracks (
                            UserId INTEGER NOT NULL,
                            TrackId INTEGER NOT NULL,
                            PRIMARY KEY (UserId, TrackId),
                            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                            FOREIGN KEY (TrackId) REFERENCES Tracks(Id) ON DELETE CASCADE
                        );
                    ";
                    createUserLikedTracksTableCommand.ExecuteNonQuery();
                    Console.WriteLine("[DatabaseSetup] UserLikedTracks table ensured.");

                    // Попытка добавить колонку Duration, если она отсутствует
                    var columnExistsCmd = connection.CreateCommand();
                    columnExistsCmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Tracks') WHERE name='Duration';";
                    bool durationColumnExists = Convert.ToInt32(columnExistsCmd.ExecuteScalar()) > 0;

                    if (!durationColumnExists)
                    {
                        Console.WriteLine("[DatabaseSetup] Column 'Duration' does not exist in 'Tracks'. Adding it...");
                        var alterDurationCmd = connection.CreateCommand();
                        alterDurationCmd.CommandText = "ALTER TABLE Tracks ADD COLUMN Duration REAL NOT NULL DEFAULT 0;";
                        alterDurationCmd.ExecuteNonQuery();
                        Console.WriteLine("[DatabaseSetup] Column 'Duration' added.");
                    } else {
                        Console.WriteLine("[DatabaseSetup] Column 'Duration' already exists in 'Tracks'.");
                    }
                     // Добавим Album и Genre если отсутствуют
                    columnExistsCmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Tracks') WHERE name='Album';";
                    if (Convert.ToInt32(columnExistsCmd.ExecuteScalar()) == 0)
                    {
                        Console.WriteLine("[DatabaseSetup] Column 'Album' does not exist in 'Tracks'. Adding it...");
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = "ALTER TABLE Tracks ADD COLUMN Album TEXT NULLABLE;";
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("[DatabaseSetup] Column 'Album' added.");
                    } else {
                        Console.WriteLine("[DatabaseSetup] Column 'Album' already exists in 'Tracks'.");
                    }

                    columnExistsCmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Tracks') WHERE name='Genre';";
                    if (Convert.ToInt32(columnExistsCmd.ExecuteScalar()) == 0)
                    {
                         Console.WriteLine("[DatabaseSetup] Column 'Genre' does not exist in 'Tracks'. Adding it...");
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = "ALTER TABLE Tracks ADD COLUMN Genre TEXT NULLABLE;";
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("[DatabaseSetup] Column 'Genre' added.");
                    } else {
                        Console.WriteLine("[DatabaseSetup] Column 'Genre' already exists in 'Tracks'.");
                    }

                    Console.WriteLine("[DatabaseSetup] Database initialized successfully.");
                }
            }
            catch (SQLiteException sqliteEx)
            {
                Console.WriteLine($"[DatabaseSetup] CRITICAL SQLITE ERROR: {sqliteEx.ToString()}");
                Console.WriteLine($"[DatabaseSetup] SQLite Error Code: {sqliteEx.ErrorCode}");
                // Пробрасываем исключение дальше, чтобы приложение не продолжило работу с неинициализированной БД
                throw; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseSetup] CRITICAL GENERAL ERROR: {ex.ToString()}");
                throw; // Пробрасываем исключение дальше
            }
        }
    }
} 