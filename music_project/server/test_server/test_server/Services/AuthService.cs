using System.Data.SQLite;
using test_server.Data; // Изменено с MusicCloud.Server.Data
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace test_server.Services // Изменено с MusicCloud.Server.Services
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService(ConnectionStringHolder connectionStringHolder)
        {
            _connectionString = connectionStringHolder.ConnectionString;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private string GenerateSimpleToken()
        {
            // Простой токен для примера. В реальном приложении использовать JWT.
            return Guid.NewGuid().ToString();
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(string nickname, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Nickname, email, and password cannot be empty.", null);
            }

            // TODO: Добавить валидацию формата email

            var passwordHash = HashPassword(password);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Проверка, существует ли пользователь с таким Nickname или Email
                var checkUserCommand = connection.CreateCommand();
                checkUserCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE Nickname = $nickname OR Email = $email";
                checkUserCommand.Parameters.AddWithValue("$nickname", nickname);
                checkUserCommand.Parameters.AddWithValue("$email", email);
                var userExists = Convert.ToInt32(await checkUserCommand.ExecuteScalarAsync()) > 0;

                if (userExists)
                {
                    // Можно добавить более конкретное сообщение, какое поле уже занято, если нужно
                    return (false, "User with this nickname or email already exists.", null);
                }

                // Создание нового пользователя
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Users (Nickname, Email, PasswordHash)
                    VALUES ($nickname, $email, $passwordHash);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("$nickname", nickname);
                command.Parameters.AddWithValue("$email", email);
                command.Parameters.AddWithValue("$passwordHash", passwordHash);
                
                var userId = Convert.ToInt32(await command.ExecuteScalarAsync());

                var newUser = new User { Id = userId, Nickname = nickname, Email = email, PasswordHash = passwordHash };
                return (true, "User registered successfully.", newUser);
            }
        }

        public async Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(string nickname, string password)
        {
            if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Nickname and password cannot be empty.", null, null);
            }

            var passwordHash = HashPassword(password);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Nickname, Email, PasswordHash, AuthToken FROM Users WHERE Nickname = $nickname AND PasswordHash = $passwordHash";
                command.Parameters.AddWithValue("$nickname", nickname);
                command.Parameters.AddWithValue("$passwordHash", passwordHash);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var user = new User
                        {
                            Id = reader.GetInt32(0),
                            Nickname = reader.GetString(1),
                            Email = reader.GetString(2),
                            PasswordHash = reader.GetString(3),
                            AuthToken = reader.IsDBNull(4) ? null : reader.GetString(4)
                        };

                        // Генерация и сохранение (или обновление) токена
                        var token = GenerateSimpleToken();
                        var updateTokenCommand = connection.CreateCommand();
                        updateTokenCommand.CommandText = "UPDATE Users SET AuthToken = $authToken WHERE Id = $id";
                        updateTokenCommand.Parameters.AddWithValue("$authToken", token);
                        updateTokenCommand.Parameters.AddWithValue("$id", user.Id);
                        await updateTokenCommand.ExecuteNonQueryAsync();
                        
                        user.AuthToken = token; // Обновляем токен в объекте пользователя

                        return (true, "Login successful.", token, user);
                    }
                }
            }
            return (false, "Неверный никнейм или пароль.", null, null);
        }

        public async Task<User?> GetUserByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Nickname, Email, PasswordHash, AuthToken FROM Users WHERE AuthToken = $token";
                command.Parameters.AddWithValue("$token", token);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Nickname = reader.GetString(1),
                            Email = reader.GetString(2),
                            PasswordHash = reader.GetString(3),
                            AuthToken = reader.GetString(4)
                        };
                    }
                }
            }
            return null;
        }
    }
} 