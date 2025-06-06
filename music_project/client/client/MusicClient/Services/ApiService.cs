using System;
using System.Collections.Generic; // Для IEnumerable<T>
using System.IO; // Для Stream в UploadTrackAsync
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 
using MusicClient.Models; 
using System.Diagnostics; 


namespace MusicClient.Services
{
    public class ApiService : IAsyncDisposable
    {
        private readonly HttpClient _httpClientInternal;
        public HttpClient HttpClient => _httpClientInternal;
        private HubConnection _hubConnection;
        private string _jwtToken;
        private bool _isDisposed = false; // Флаг состояния Dispose

      
        private const string BaseUrl = "http://192.168.1.224:5063";

        // События для стриминга
        public event Func<TrackDto, Task> OnTrackInfoReceived;
        public event Func<byte[], Task> OnAudioChunkReceived;
        public event Func<Task> OnStreamFinished;
        public event Func<string, Task> OnStreamError;

        public ApiService()
        {
            _httpClientInternal = new HttpClient();
            _httpClientInternal.BaseAddress = new Uri(BaseUrl);
            _httpClientInternal.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClientInternal.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public bool IsUserLoggedIn => !string.IsNullOrEmpty(_jwtToken);

        public async Task<IEnumerable<TrackDto>> GetAllTracksAsync()
        {
            try
            {
                var response = await _httpClientInternal.GetAsync("api/tracks");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<TrackDto>>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка GetAllTracksAsync: {ex.Message}");
                return new List<TrackDto>();
            }
        }

        public async Task<IEnumerable<TrackDto>> GetLikedTracksAsync()
        {
            try
            {
                var response = await _httpClientInternal.GetAsync("api/userlikedtracks");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<TrackDto>>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка GetLikedTracksAsync: {ex.Message}");
                return new List<TrackDto>();
            }
        }

        public async Task<IEnumerable<PlaylistDto>> GetUserPlaylistsAsync()
        {
            try
            {
                var response = await _httpClientInternal.GetAsync("api/playlists");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<PlaylistDto>>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка GetUserPlaylistsAsync: {ex.Message}");
                return new List<PlaylistDto>();
            }
        }
        
        public async Task<PlaylistDto> GetPlaylistAsync(int playlistId)
        {
            try
            {
                var response = await _httpClientInternal.GetAsync($"api/playlists/{playlistId}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PlaylistDto>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка GetPlaylistAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<TrackDto>> SearchTracksAsync(string query)
        {
            try
            {
                var response = await _httpClientInternal.GetAsync($"api/tracks/search?query={Uri.EscapeDataString(query)}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<TrackDto>>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка SearchTracksAsync: {ex.Message}");
                return new List<TrackDto>();
            }
        }

        public async Task<bool> LikeTrackAsync(int trackId)
        {
            try
            {
                var response = await _httpClientInternal.PostAsync($"api/userlikedtracks/like/{trackId}", null);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ApiService] LikeTrackAsync для ID {trackId} провален. Статус: {response.StatusCode}. Ответ: {errorContent}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Исключение в LikeTrackAsync для ID {trackId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnlikeTrackAsync(int trackId)
        {
            try
            {
                var response = await _httpClientInternal.PostAsync($"api/userlikedtracks/unlike/{trackId}", null);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ApiService] UnlikeTrackAsync для ID {trackId} провален. Статус: {response.StatusCode}. Ответ: {errorContent}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiService] Исключение в UnlikeTrackAsync для ID {trackId}: {ex.Message}");
                return false;
            }
        }
        
        public async Task<TrackDto> UploadTrackAsync(Stream fileStream, string fileName, string title, string artist, string album, string genre, string contentType = "application/octet-stream")
        {
            if (_isDisposed)
            {
                Console.WriteLine("[ApiService] UploadTrackAsync вызван для уничтоженного объекта ApiService.");
                throw new ObjectDisposedException(nameof(ApiService));
            }
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray(); // Получаем байты

                    using (var content = new MultipartFormDataContent())
                    // Удаляем using для streamContent, так как ByteArrayContent будет добавлен напрямую
                    {
                        var fileContent = new ByteArrayContent(fileBytes); // Используем ByteArrayContent
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                        
                        // LoadIntoBufferAsync() не нужен для ByteArrayContent

                        content.Add(fileContent, "file", fileName);
                        content.Add(new StringContent(title ?? string.Empty), "title");
                        content.Add(new StringContent(artist ?? string.Empty), "artist");
                        content.Add(new StringContent(album ?? string.Empty), "album");
                        content.Add(new StringContent(genre ?? string.Empty), "genre");

                        var response = await _httpClientInternal.PostAsync("api/tracks/upload", content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<TrackDto>(responseContent);
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"[ApiService] Upload failed: {response.StatusCode} - {errorContent}");
                            // Попробуем десериализовать ошибку, если это JSON
                            try
                            {
                                var errorDetails = JsonConvert.DeserializeObject<object>(errorContent); // или конкретный тип ошибки
                                Console.WriteLine($"[ApiService] Server error details: {JsonConvert.SerializeObject(errorDetails, Formatting.Indented)}");
                            }
                            catch { /* Не удалось десериализовать, просто вывели как строку */ }
                            return null;
                        }
                    }
                }
            }
            catch (ObjectDisposedException odEx)
            {
                Console.WriteLine($"[ApiService] ObjectDisposedException в UploadTrackAsync: {odEx.ToString()} (Object Name: {odEx.ObjectName})");
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[ApiService] HttpRequestException в UploadTrackAsync: {httpEx.ToString()}");
                if (httpEx.InnerException != null)
                {
                    Console.WriteLine($"[ApiService] Inner HttpRequestException: {httpEx.InnerException.ToString()}");
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Критическая ошибка в UploadTrackAsync: {ex.ToString()}");
                return null;
            }
        }

        public async Task<PlaylistDto> CreatePlaylistAsync(string name)
        {
            try
            {
                var createPlaylistDto = new CreatePlaylistDto { Name = name };
                var json = JsonConvert.SerializeObject(createPlaylistDto);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClientInternal.PostAsync("api/playlists", stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PlaylistDto>(responseContent);
                }
                Console.WriteLine($"Ошибка создания плейлиста: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка CreatePlaylistAsync: {ex.Message}");
                return null;
            }
        }

        // TODO: Методы для инициализации SignalR, подключения, вызова серверных методов хаба
        // и обработки сообщений от хаба

        public async Task InitializeHubConnectionAsync()
        {
            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                Console.WriteLine("Hub connection already initialized or connected.");
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{BaseUrl}/streaminghub", options =>
                {
                    if (!string.IsNullOrEmpty(_jwtToken))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
                    }
                })
                .WithAutomaticReconnect()
                .Build();
            
            // Регистрируем обработчики сообщений от хаба
            _hubConnection.On<TrackDto>("ReceiveTrackInfo", async (trackInfo) =>
            {
                if (OnTrackInfoReceived != null)
                {
                    await OnTrackInfoReceived.Invoke(trackInfo);
                }
            });

            _hubConnection.On<byte[]>("ReceiveAudioChunk", async (chunk) =>
            {
                if (OnAudioChunkReceived != null)
                {
                    await OnAudioChunkReceived.Invoke(chunk);
                }
            });

            _hubConnection.On("StreamFinished", async () =>
            {
                if (OnStreamFinished != null)
                {
                    await OnStreamFinished.Invoke();
                }
            });

            _hubConnection.On<string>("StreamError", async (errorMessage) =>
            {
                if (OnStreamError != null)
                {
                    await OnStreamError.Invoke(errorMessage);
                }
            });

            _hubConnection.Closed += async (error) =>
            {
                // Это событие вызывается, когда соединение закрыто.
                // Можно добавить логику для обработки неожиданного закрытия.
                Console.WriteLine($"SignalR Hub connection closed. Error: {error?.Message}");
                // Возможно, потребуется попытаться переподключиться или уведомить пользователя.
                // WithAutomaticReconnect() должен обрабатывать большинство случаев, но здесь можно добавить доп. логику.
                await Task.CompletedTask; // Просто для примера, чтобы сделать лямбду async
            };

            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR Hub connected successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
        }

        public async Task StartStreamingTrackAsync(int trackId) // Переименовал для ясности, что это команда к хабу
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("StreamTrack", trackId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking StreamTrack on hub: {ex.Message}");
                    if(OnStreamError != null) await OnStreamError.Invoke($"Failed to start streaming: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("SignalR Hub not connected. Cannot stream track.");
                if(OnStreamError != null) await OnStreamError.Invoke("Not connected to streaming server.");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;

            if (_hubConnection != null)
            {
                // Отписываемся от обработчиков, чтобы избежать утечек памяти, если ApiService будет пересоздаваться
                _hubConnection.Remove("ReceiveTrackInfo");
                _hubConnection.Remove("ReceiveAudioChunk");
                _hubConnection.Remove("StreamFinished");
                _hubConnection.Remove("StreamError");
                // _hubConnection.Closed -= ... ; // Если бы мы присваивали именованный метод

                await _hubConnection.DisposeAsync();
                _hubConnection = null; // Убедимся, что ссылка обнулена
            }
            _httpClientInternal.Dispose();
            _isDisposed = true; // Устанавливаем флаг
            GC.SuppressFinalize(this); // Если есть финализатор, хотя для этого класса он не нужен
        }

        public async Task<(bool Success, string Message, AuthResponseDto AuthResponse)> RegisterAsync(UserRegisterDto registerDto)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(ApiService));
            try
            {
                var json = JsonConvert.SerializeObject(registerDto); // registerDto уже содержит Nickname, Email, Password
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClientInternal.PostAsync("api/auth/register", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Ответ от сервера может не содержать полный AuthResponseDto при регистрации,
                    // а только сообщение и, возможно, Id пользователя.
                    // Пока оставим десериализацию в AuthResponseDto, но это можно изменить.
                    var authResponse = JsonConvert.DeserializeObject<AuthResponseDto>(responseContent);
                    // При успешной регистрации токен обычно не возвращается сразу, пользователь должен войти отдельно.
                    // Так что SetToken(authResponse.Token) здесь может быть лишним.
                    // Если сервер возвращает токен при регистрации, то можно оставить.
                    // В текущей реализации сервера (Program.cs) токен НЕ возвращается при регистрации.
                    return (true, "Registration successful!", authResponse); // authResponse может быть частично пустым
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string message = $"Registration failed. Status: {response.StatusCode}";
                    try 
                    {
                        var errorJObject = JsonConvert.DeserializeObject<JObject>(errorContent);
                        string extractedMessage = errorJObject?["message"]?.ToString() ?? 
                                                  errorJObject?["title"]?.ToString();
                        if (!string.IsNullOrEmpty(extractedMessage)) message = extractedMessage;
                        else message = errorContent; 
                        var errorsToken = errorJObject?["errors"];
                        if (errorsToken != null)
                        {
                            message += " Details: " + errorsToken.ToString(Newtonsoft.Json.Formatting.None);
                        }
                    }
                    catch { /* Оставляем message как есть */ }
                    return (false, message, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка RegisterAsync: {ex.Message}");
                return (false, $"Ошибка регистрации: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, AuthResponseDto AuthResponse)> LoginAsync(UserLoginDto loginDto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(loginDto); // loginDto уже содержит Nickname, Password
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClientInternal.PostAsync("api/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonConvert.DeserializeObject<AuthResponseDto>(responseContent);
                    if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
                    {
                        SetToken(authResponse.Token);
                        return (true, "Login successful!", authResponse);
                    }
                    return (false, "Login failed: Invalid response from server.", null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string message = $"Login failed. Status: {response.StatusCode}"; // Сообщение по умолчанию
                    try 
                    {
                        var errorJObject = JsonConvert.DeserializeObject<JObject>(errorContent);
                        string extractedMessage = errorJObject?["message"]?.ToString() ?? 
                                                  errorJObject?["title"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(extractedMessage))
                        {
                            message = extractedMessage;
                        }
                        // Если extractedMessage пуст, а errorContent не является валидным JSON с ошибкой, 
                        // или сам errorContent пуст/содержит только пробелы, 
                        // то message останется "Login failed. Status: {response.StatusCode}" 
                        // или будет заменено на errorContent, если он не пустой и не JSON.
                        // Чтобы избежать пустого окна, если errorContent пуст или не JSON и не содержит текста:
                        else if (string.IsNullOrWhiteSpace(errorContent) || !errorContent.TrimStart().StartsWith("{"))
                        {
                            // Оставляем message = $"Login failed. Status: {response.StatusCode}";
                            // или можно установить более общее сообщение, если errorContent бесполезен
                        }
                        else // errorContent есть, не пустой, и может быть JSON, но без message/title
                        {
                             // Попытка взять весь errorContent, если он не пустой и предположительно JSON
                             // Но если он не содержит полезной текстовой информации, лучше оставить дефолтное
                             if (!string.IsNullOrWhiteSpace(errorContent))
                             {
                                // Проверим, не является ли errorContent слишком большим или HTML
                                if (errorContent.Length < 500 && !errorContent.TrimStart().StartsWith("<"))
                                {
                                    message = errorContent;
                                }
                                // Иначе, оставляем дефолтное сообщение со статусом.
                             }
                        }

                        var errorsToken = errorJObject?["errors"];
                        if (errorsToken != null)
                        {
                            // Добавляем детали, если они есть и сообщение не стало слишком длинным
                            string details = errorsToken.ToString(Newtonsoft.Json.Formatting.None);
                            if (message.Length + details.Length < 1000) // Ограничение на длину сообщения
                            {
                                message += " Details: " + details;
                            }
                        }
                    }
                    catch 
                    { 
                        // Если парсинг JSON не удался, и errorContent не пустой и не HTML, используем его.
                        // Иначе, останется сообщение по умолчанию.
                        if (!string.IsNullOrWhiteSpace(errorContent) && errorContent.Length < 500 && !errorContent.TrimStart().StartsWith("<"))
                        {
                            message = errorContent;
                        }
                    }
                    // Финальная проверка, чтобы сообщение не было пустым
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = $"Login failed with status {response.StatusCode}. No further details available.";
                    }
                    return (false, message, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка LoginAsync: {ex.Message}");
                return (false, $"Ошибка входа: {ex.Message}", null);
            }
        }

        public void Logout()
        {
            _jwtToken = null;
            _httpClientInternal.DefaultRequestHeaders.Authorization = null;
            _hubConnection?.StopAsync();
        }

        public async Task<Stream> GetTrackStreamAsync(int trackId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/tracks/stream/{trackId}");
                var response = await _httpClientInternal.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Ошибка GetTrackStreamAsync: {ex.Message}");
                return Stream.Null;
            }
        }

        public async Task<(bool Success, string Message)> DeleteTrackAsync(int trackId)
        {
            if (_isDisposed)
            {
                Console.WriteLine("[ApiService] DeleteTrackAsync вызван для уничтоженного объекта ApiService.");
                // Consider throwing ObjectDisposedException or returning a specific error tuple
                return (false, "ApiService has been disposed.");
            }
            try
            {
                var response = await _httpClientInternal.DeleteAsync($"api/tracks/{trackId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Пытаемся извлечь сообщение об успехе, если оно есть
                    // Сервер возвращает объект { message: "..." }
                    dynamic? result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string message = result?.message?.ToString() ?? "Трек успешно удален.";
                    return (true, message);
                }
                else
                {
                    // Пытаемся извлечь сообщение об ошибке
                    string errorMessage = "Неизвестная ошибка при удалении трека.";
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        try
                        {
                            dynamic? errorResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                            errorMessage = errorResult?.message?.ToString() ?? errorResult?.detail?.ToString() ?? $"Ошибка: {response.StatusCode}";
                        }
                        catch 
                        { 
                            // Если ответ не JSON, используем статус код
                            errorMessage = $"Ошибка сервера: {response.StatusCode}, Ответ: {responseContent}";
                        }
                    }
                    else
                    {
                        errorMessage = $"Ошибка сервера: {response.StatusCode}";
                    }
                    Console.WriteLine($"[ApiService] Ошибка DeleteTrackAsync ({trackId}): {response.StatusCode} - {errorMessage}");
                    return (false, errorMessage);
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[ApiService] HTTP Исключение в DeleteTrackAsync ({trackId}): {httpEx.Message}");
                return (false, $"Ошибка сети при удалении: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"[ApiService] JSON Исключение в DeleteTrackAsync ({trackId}): {jsonEx.Message}");
                return (false, $"Ошибка обработки ответа сервера: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService] Общее Исключение в DeleteTrackAsync ({trackId}): {ex.Message}");
                return (false, $"Непредвиденная ошибка: {ex.Message}");
            }
        }
    }
} 