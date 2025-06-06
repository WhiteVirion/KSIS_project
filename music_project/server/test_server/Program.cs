using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MusicCloud.Server.Data; // Предполагая, что DatabaseSetup будет в этом namespace
using MusicCloud.Server.Services; // Добавляем using для сервисов

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();

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
app.MapPost("/api/auth/register", async (UserRegistrationRequest request, AuthService authService) => {
    // Проверка входных данных
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Имя пользователя и пароль не могут быть пустыми.");
    }

    // Вызов сервиса для регистрации
    var (success, message, user) = await authService.RegisterAsync(request.Username, request.Password);

    if (!success)
    {
        // Если регистрация не удалась (например, пользователь уже существует), возвращаем ошибку
        // Можно использовать Results.Conflict, если пользователь уже существует
        return Results.BadRequest(new { Message = message });
    }

    // Возвращаем успешный результат (без данных пользователя, чтобы не светить хеш пароля и токен, если он там есть)
    return Results.Ok(new { Message = message });
});

// Эндпоинт для входа пользователя
app.MapPost("/api/auth/login", async (UserLoginRequest request, AuthService authService) => {
    // Проверка входных данных
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Имя пользователя и пароль не могут быть пустыми.");
    }

    // Вызов сервиса для входа
    var (success, message, token, user) = await authService.LoginAsync(request.Username, request.Password);

    if (!success)
    {
        // Если вход не удался (неверные учетные данные), возвращаем ошибку
        return Results.Unauthorized(); // Стандартный ответ для неудачной аутентификации
    }

    // Возвращаем токен доступа в случае успеха
    return Results.Ok(new { Token = token, UserId = user?.Id, Username = user?.Username }); // Возвращаем также ID и имя пользователя
});

// API для управления треками

// Эндпоинт для загрузки нового аудиофайла
// Защищено: требует валидный токен
app.MapPost("/api/tracks/upload", async (HttpRequest httpRequest, TrackService trackService) => {
    // Пытаемся получить пользователя из контекста запроса (добавленного middleware)
    var user = httpRequest.HttpContext.Items["User"] as User;
    if (user == null)
    {
        return Results.Unauthorized(); // Если пользователя нет (токен неверный или отсутствует), возвращаем 401
    }

    if (!httpRequest.HasFormContentType)
    {
        return Results.BadRequest("Неверный тип контента. Ожидается multipart/form-data.");
    }

    var form = await httpRequest.ReadFormAsync();
    var file = form.Files.GetFile("file"); // Клиент должен отправить файл с именем "file"
    // Клиент может также передать title и artist как части формы
    var title = form["title"].FirstOrDefault();
    var artist = form["artist"].FirstOrDefault();

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("Файл не загружен или пуст.");
    }

    var (success, message, newTrack) = await trackService.UploadTrackAsync(file, user.Id, title, artist);

    if (!success)
    {
        return Results.Problem(detail: message, statusCode: StatusCodes.Status500InternalServerError); // Или Results.BadRequest(new { Message = message });
    }

    return Results.Ok(new { Message = message, Track = newTrack });
}).DisableAntiforgery();

// Эндпоинт для получения списка всех треков
// Защищено: требует валидный токен
app.MapGet("/api/tracks", async (TrackService trackService, HttpContext httpContext) => {
    if (httpContext.Items["User"] == null)
    {
        return Results.Unauthorized();
    }

    var tracks = await trackService.GetAllTracksAsync();
    // Для клиента может быть полезно не возвращать полный FilePath и UserId, а только нужную информацию.
    // Здесь мы возвращаем все, как есть в модели Track, но можно создать DTO.
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
public record UserRegistrationRequest(string Username, string Password);
public record UserLoginRequest(string Username, string Password);
