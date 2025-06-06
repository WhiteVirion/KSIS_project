using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using test_server.Data; // Изменено с MusicCloud.Server.Data
using test_server.Services; // Изменено с MusicCloud.Server.Services
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http.Features; // Для FormOptions и RequestFormLimits
using Microsoft.AspNetCore.Server.Kestrel.Core; // Для KestrelServerLimits

var builder = WebApplication.CreateBuilder(args);

// Увеличение лимитов на размер запроса (глобально)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
    options.ValueLengthLimit = 100 * 1024 * 1024; // 100MB
    options.MemoryBufferThreshold = 100 * 1024 * 1024; // 100MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

// Добавление сервисов в контейнер.
// Здесь можно будет добавить сервисы для аутентификации, работы с треками и т.д.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Опционально, для тестирования API через Swagger UI

// Сервис для инициализации БД
builder.Services.AddSingleton<DatabaseSetup>(); 
// Строка подключения к SQLite. Файл БД будет создан в корневой директории сервера.
builder.Services.AddSingleton(new ConnectionStringHolder("Data Source=music_library.db"));

// Регистрация AuthService в DI контейнере
// AddScoped подходит для сервисов, которые могут использовать ресурсы, специфичные для запроса (например, DbContext)
// или если у них есть состояние, которое не должно разделяться между разными запросами.
// В нашем случае AuthService взаимодействует с БД напрямую через SqliteConnection,
// и каждый метод открывает и закрывает соединение, так что AddScoped или AddTransient будут уместны.
// AddSingleton также возможен, если сервис полностью stateless и потокобезопасен.
builder.Services.AddScoped<AuthService>();

// Регистрация TrackService в DI контейнере
// TrackService использует IWebHostEnvironment, который обычно регистрируется как Singleton,
// и ConnectionStringHolder (Singleton). Сам TrackService также может быть Scoped или Singleton.
// Выберем Scoped для консистентности с AuthService.
builder.Services.AddScoped<TrackService>();

var app = builder.Build();

// Инициализация базы данных при старте приложения
var dbInitializer = app.Services.GetRequiredService<DatabaseSetup>();
dbInitializer.Initialize();

// Конфигурация HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // УБЕДИТЕСЬ, ЧТО ЭТО ЗАКОММЕНТИРОВАНО

// Middleware для простой проверки токена (должен быть добавлен перед app.Map... для защищенных эндпоинтов)
// Этот middleware будет пытаться извлечь пользователя по токену и добавлять его в HttpContext.Items
// для последующего использования в эндпоинтах.
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
    if (token != null)
    {
        var authService = context.RequestServices.GetRequiredService<AuthService>();
        var user = await authService.GetUserByTokenAsync(token);
        if (user != null)
        {
            context.Items["User"] = user; // Сохраняем пользователя в контексте запроса
        }
    }
    await next();
});

// API для аутентификации пользователей
// Эндпоинт для регистрации нового пользователя
app.MapPost("/api/auth/register", async (UserRegisterRequest req, AuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(req.Nickname) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
    {
        return Results.BadRequest(new { message = "Nickname, email, and password are required." });
    }
    var (success, message, user) = await authService.RegisterAsync(req.Nickname, req.Email, req.Password);
    if (success)
    {
        return Results.Ok(new { message, userId = user?.Id, nickname = user?.Nickname, email = user?.Email });
    }
    return Results.BadRequest(new { message });
});

// Эндпоинт для входа пользователя
app.MapPost("/api/auth/login", async (UserLoginRequest req, AuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(req.Nickname) || string.IsNullOrWhiteSpace(req.Password))
    {
        return Results.BadRequest(new { message = "Nickname and password are required." });
    }
    var (success, message, token, user) = await authService.LoginAsync(req.Nickname, req.Password);
    if (success && token != null && user != null)
    {
        return Results.Ok(new { message, token, userId = user.Id, nickname = user.Nickname, email = user.Email });
    }
    return Results.Json(new  { message }, statusCode: StatusCodes.Status401Unauthorized);
});

// API для управления треками

// Эндпоинт для загрузки нового аудиофайла
// Защищено: требует валидный токен
app.MapPost("/api/tracks/upload", 
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)] // Лимит 100MB для этого эндпоинта
    async (
        [FromForm] IFormFile file,
        [FromForm] string? title,
        [FromForm] string? artist,
        HttpContext httpContext,
        TrackService trackService
    ) => {
    var user = httpContext.Items["User"] as User;
    if (user == null)
        return Results.Unauthorized();

    if (file == null || file.Length == 0)
        return Results.BadRequest("Файл не загружен или пуст.");

    // Проверка расширения файла
    var allowedExtensions = new[] { ".mp3", ".wav", ".ogg" };
    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(ext))
        return Results.BadRequest("Недопустимый тип файла. Разрешены только mp3, wav, ogg.");

    // Проверка MIME-типа
    var allowedMimeTypes = new[] { "audio/mpeg", "audio/wav", "audio/ogg" };
    if (!allowedMimeTypes.Contains(file.ContentType))
        return Results.BadRequest($"Недопустимый MIME-тип файла: {file.ContentType}");

    var (success, message, newTrack) = await trackService.UploadTrackAsync(file, user.Id, title, artist);

    if (!success)
        return Results.Problem(detail: message, statusCode: StatusCodes.Status500InternalServerError);

    return Results.Ok(new { Message = message, Track = newTrack });
}).DisableAntiforgery();

// Эндпоинт для получения списка всех треков
// Защищено: требует валидный токен
app.MapGet("/api/tracks", async (TrackService trackService, HttpContext httpContext) => {
    var user = httpContext.Items["User"] as User; // Получаем пользователя
    if (user == null) // Проверка пользователя
    {
        return Results.Unauthorized();
    }

    var tracks = await trackService.GetAllTracksAsync(user.Id); // Передаем user.Id
    return Results.Ok(tracks);
});

// Эндпоинт для потокового воспроизведения трека
// Защищено: требует валидный токен
app.MapGet("/api/tracks/stream/{trackId}", async (int trackId, TrackService trackService, HttpContext httpContext) => {
    if (httpContext.Items["User"] == null)
    {
        return Results.Unauthorized();
    }

    if (trackId <= 0)
    {
        return Results.BadRequest("Некорректный ID трека.");
    }

    var track = await trackService.GetTrackByIdAsync(trackId);
    if (track == null || string.IsNullOrEmpty(track.FilePath) || !System.IO.File.Exists(track.FilePath))
    {
        return Results.NotFound("Трек не найден или файл отсутствует на сервере.");
    }

    // Возвращаем файл как поток. 
    // ContentType можно определять более точно на основе расширения файла, если нужно.
    // EnableRangeProcessing позволяет клиенту запрашивать части файла (для перемотки).
    return Results.File(track.FilePath, contentType: "application/octet-stream", enableRangeProcessing: true, fileDownloadName: track.FileName);
});

// Эндпоинт для поиска треков
// Защищено: требует валидный токен
app.MapGet("/api/tracks/search", async (string query, TrackService trackService, HttpContext httpContext) => {
    var user = httpContext.Items["User"] as User;
    if (user == null)
        return Results.Unauthorized();

    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest("Поисковый запрос не может быть пустым.");

    var tracks = await trackService.SearchTracksAsync(query, user.Id); // Передаем userId для потенциального поиска только по трекам пользователя
    return Results.Ok(tracks);
});

// Эндпоинт для удаления трека по ID
// Защищено: требует валидный токен
app.MapDelete("/api/tracks/{trackId}", async (int trackId, TrackService trackService, HttpContext httpContext) => {
    var user = httpContext.Items["User"] as User;
    if (user == null) 
        return Results.Unauthorized();

    if (trackId <= 0)
        return Results.BadRequest("Некорректный ID трека.");

    var (success, message) = await trackService.DeleteTrackAsync(trackId, user.Id);
    
    if (!success)
    {
        if (message.Contains("не найден"))
            return Results.NotFound(new { message });
        if (message.Contains("Доступ запрещен"))
            return Results.Forbid(); // 403 Forbidden
        // Для других ошибок (включая "Внутренняя ошибка сервера")
        return Results.Problem(detail: message, statusCode: StatusCodes.Status500InternalServerError);
    }

    return Results.Ok(new { message });
});

// Эндпоинт для добавления трека в понравившиеся (лайк)
// Защищено: требует валидный токен
app.MapPost("/api/userlikedtracks/like/{trackId}", async (int trackId, TrackService trackService, HttpContext httpContext) => {
    var user = httpContext.Items["User"] as User; // Предполагается, что класс User существует и имеет свойство Id
    if (user == null)
        return Results.Unauthorized();

    if (trackId <= 0)
        return Results.BadRequest("Некорректный ID трека.");

    // Предполагается, что TrackService будет иметь метод LikeTrackAsync(int userId, int trackId)
    var success = await trackService.LikeTrackAsync(user.Id, trackId); 
    
    if (!success)
    {
        // В идеале, TrackService должен возвращать более конкретную информацию об ошибке
        return Results.Problem(detail: "Не удалось добавить трек в понравившиеся.", statusCode: StatusCodes.Status500InternalServerError);
    }

    return Results.Ok(new { message = "Трек успешно добавлен в понравившиеся." });
});

// Эндпоинт для удаления трека из понравившихся (анлайк)
// Защищено: требует валидный токен
app.MapPost("/api/userlikedtracks/unlike/{trackId}", async (int trackId, TrackService trackService, HttpContext httpContext) => {
    var user = httpContext.Items["User"] as User;
    if (user == null)
        return Results.Unauthorized();

    if (trackId <= 0)
        return Results.BadRequest("Некорректный ID трека.");

    var success = await trackService.UnlikeTrackAsync(user.Id, trackId);
    
    if (!success)
    {
        // UnlikeTrackAsync возвращает false, если лайка не было или при ошибке. 
        // Для клиента, возможно, не важно, был ли лайк удален или его и не было.
        // Но если была ошибка (например, БД недоступна), это проблема.
        // Можно добавить более детальную обработку ошибок из TrackService, если он будет их возвращать.
        return Results.Problem(detail: "Не удалось убрать трек из понравившихся, возможно, он не был лайкнут или произошла ошибка.", statusCode: StatusCodes.Status400BadRequest); 
    }

    return Results.Ok(new { message = "Трек успешно убран из понравившихся." });
});

// Эндпоинт для получения списка всех понравившихся треков пользователя
// Защищено: требует валидный токен
app.MapGet("/api/userlikedtracks", async (TrackService trackService, HttpContext httpContext) => {
    var user = httpContext.Items["User"] as User;
    if (user == null)
        return Results.Unauthorized();

    var likedTracks = await trackService.GetUserLikedTracksAsync(user.Id);
    return Results.Ok(likedTracks);
});

// Заглушка для эндпоинта плейлистов
// Защищено: требует валидный токен
app.MapGet("/api/playlists", (HttpContext httpContext) => {
    if (httpContext.Items["User"] == null)
    {
        return Results.Unauthorized();
    }
    // Пока возвращаем пустой список или NoContent
    return Results.Ok(new List<object>()); 
});

app.Run();

// Вспомогательный класс для хранения строки подключения, чтобы ее можно было передать в DatabaseSetup
// Вы можете разместить его в отдельном файле или здесь же, если он используется только локально.
public class ConnectionStringHolder
{
    public string ConnectionString { get; }
    public ConnectionStringHolder(string connectionString)
    {
        ConnectionString = connectionString;
    }
}

// DTOs for requests - can be moved to separate files later
public record UserRegisterRequest(string Nickname, string Email, string Password);
public record UserLoginRequest(string Nickname, string Password);
