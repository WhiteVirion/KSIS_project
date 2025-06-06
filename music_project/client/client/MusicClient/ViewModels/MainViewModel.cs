using System;
using System.Collections.Generic; // Для List<byte>
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO; // Для MemoryStream
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MusicClient.Models;
using MusicClient.Services;
using MusicClient.Views;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Diagnostics; // <--- ДОБАВИТЬ

namespace MusicClient.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IAsyncDisposable
    {
        private readonly ApiService _apiService;
        private readonly AuthResponseDto _currentUser;
        private TrackDto _selectedTrack;
        private bool _isPlaying;
        private string _nowPlayingText;
        private bool _isLoading;
        private object _currentView;
        private int? _currentlyLoadedTrackId = null; // ID трека, который сейчас загружен в CurrentAudioFilePath

        // Состояния для стриминга
        private TrackDto _streamingTrackInfo; // Информация о треке, который стримится
        private List<byte> _audioBuffer = new List<byte>();
        private bool _isStreamingActive = false;
        private bool _isBuffering = false;
        private string _streamErrorMessage = string.Empty;

        // Свойство для текущего потока аудиоданных, который будет использоваться плеером
        // Это свойство будет установлено, когда стриминг завершен и данные готовы
        private MemoryStream _currentAudioStream;
        public MemoryStream CurrentAudioStream 
        {
            get => _currentAudioStream;
            private set
            {
                _currentAudioStream = value;
                OnPropertyChanged();
            }
        }
        
        public object CurrentView 
        {
            get => _currentView;
            private set { _currentView = value; OnPropertyChanged(); }
        }
        public ICommand ShowAllTracksCommand { get; private set; }
        // public ICommand ShowSearchCommand { get; private set; } // <-- ЗАКОММЕНТИРОВАТЬ ЭТУ СТРОКУ
        public ICommand ShowFavoritesCommand { get; private set; }
        public ICommand ShowProfileCommand { get; private set; }
        public ICommand ShowUploadTrackViewCommand { get; private set; }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TrackDto> AllTracks { get; private set; }
        public ObservableCollection<TrackDto> FavoriteTracks { get; private set; }
        public ObservableCollection<PlaylistDto> UserPlaylists { get; private set; }
        
        public TrackDto SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                bool isNewInstanceOfSameId = _selectedTrack != null && value != null && _selectedTrack.Id == value.Id && !ReferenceEquals(_selectedTrack, value);

                if (isNewInstanceOfSameId)
                {
                    // Это новый экземпляр DTO для того же трека.
                    // Просто обновляем ссылку и IsCurrent, не перезапуская воспроизведение.
                    if (_selectedTrack != null)
                    {
                        _selectedTrack.IsCurrent = false;
                    }
                    _selectedTrack = value;
                    if (_selectedTrack != null)
                    {
                        _selectedTrack.IsCurrent = true;
                    }
                    OnPropertyChanged();
                }
                else if (_selectedTrack != value) // Действительно другой трек или изменение на/с null
                {
                    if (_selectedTrack != null)
                    {
                        _selectedTrack.IsCurrent = false; // Снимаем выделение со старого
                    }
                    
                    var oldTrack = _selectedTrack;
                    _selectedTrack = value; // Устанавливаем новый трек

                    if (_selectedTrack != null) // Если выбран новый трек (не null)
                    {
                        _selectedTrack.IsCurrent = true;
                        NowPlayingText = $"Выбран: {_selectedTrack.Title} - {_selectedTrack.Artist}";
                        OnPropertyChanged(); // Уведомляем, что SelectedTrack изменился

                        // Важно: Если это тот же трек, что и играл (например, пользователь кликнул на него снова),
                        // то PlayCommand.Execute(null) должен просто переключить Play/Pause, а не перезагружать.
                        // TogglePlayPause должен это корректно обработать, если _currentlyLoadedTrackId совпадает.
                        // Если это действительно новый трек (ID отличается от _currentlyLoadedTrackId или oldTrack.Id),
                        // то нужно сбросить состояние и загрузить.

                        bool shouldResetAndPlay = oldTrack == null || oldTrack.Id != _selectedTrack.Id || string.IsNullOrEmpty(CurrentAudioFilePath) || _currentlyLoadedTrackId != _selectedTrack.Id;

                        if (shouldResetAndPlay)
                        {
                            IsPlaying = false; // Останавливаем предыдущий, если он играл и это был другой трек
                            CurrentAudioFilePath = null;
                            _currentlyLoadedTrackId = null;
                            CurrentPosition = 0;
                        }
                        
                        // Вызываем команду воспроизведения. TogglePlayPause разберется,
                        // нужно ли загружать трек или просто возобновить/поставить на паузу.
                        if (PlayCommand.CanExecute(null))
                        {
                            PlayCommand.Execute(null);
                        }
                    }
                    else // Новый выбранный трек - null (например, все треки удалены)
                    {
                        NowPlayingText = "Трек не выбран";
                        IsPlaying = false; 
                        CurrentAudioFilePath = null;
                        _currentlyLoadedTrackId = null;
                        CurrentPosition = 0;
                        OnPropertyChanged(); // Уведомляем, что SelectedTrack изменился (стал null)
                    }
                }
                // Если _selectedTrack == value (тот же самый экземпляр), ничего не делаем.
            }
        }
        
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    if (_isPlaying && SelectedTrack != null)
                    {
                        // _progressTimer.Start();
                    }
                    else
                    {
                        // _progressTimer.Stop();
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PlayButtonText));
                }
            }
        }
        
        public string NowPlayingText
        {
            get => _nowPlayingText;
            set { _nowPlayingText = value; OnPropertyChanged(); }
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsStreamingActive
        {
            get => _isStreamingActive;
            private set { _isStreamingActive = value; OnPropertyChanged(); }
        }

        public bool IsBuffering
        {
            get => _isBuffering;
            private set { _isBuffering = value; OnPropertyChanged(); }
        }

        public string StreamErrorMessage
        {
            get => _streamErrorMessage;
            private set { _streamErrorMessage = value; OnPropertyChanged(); }
        }
        
        public string PlayButtonText => IsPlaying ? "Пауза" : "Воспроизвести";
        
        public ICommand PlayCommand { get; private set; }
        public ICommand ToggleFavoriteCommand { get; private set; }
        public ICommand ShuffleCommand { get; private set; }
        public ICommand PreviousTrackCommand { get; private set; }
        public ICommand NextTrackCommand { get; private set; }
        public ICommand RepeatCommand { get; private set; }
        public ICommand CreatePlaylistCommand { get; private set; }
        public ICommand ShowLikedSongsCommand { get; private set; }
        public ICommand ShowRadioCommand { get; private set; }
        public ICommand OpenPlaylistCommand { get; private set; }
        public ICommand ShowLyricsCommand { get; private set; }
        public ICommand ShowQueueCommand { get; private set; }
        public ICommand VolumeCommand { get; private set; }
        public ICommand DownloadCommand { get; private set; }
        public ICommand SelectTrackCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand DeleteTrackCommand { get; private set; }
        
        private string _currentAudioFilePath;
        public string CurrentAudioFilePath
        {
            get => _currentAudioFilePath;
            set { _currentAudioFilePath = value; OnPropertyChanged(); }
        }
        
        private double _currentPosition;
        public double CurrentPosition
        {
            get => _currentPosition;
            set { _currentPosition = value; OnPropertyChanged(); }
        }

        private double _totalDuration;
        public double TotalDuration
        {
            get => _totalDuration;
            set { _totalDuration = value; OnPropertyChanged(); }
        }

        private string _currentTime;
        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        private string _totalTime;
        public string TotalTime
        {
            get => _totalTime;
            set { _totalTime = value; OnPropertyChanged(); }
        }

        private double _volume = 0.5;
        public double Volume
        {
            get => _volume;
            set { _volume = value; OnPropertyChanged(); }
        }
        
        public MainViewModel(ApiService apiService, AuthResponseDto currentUser)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            // Подписываемся на события стриминга от ApiService
            _apiService.OnTrackInfoReceived += HandleTrackInfoReceived;
            _apiService.OnAudioChunkReceived += HandleAudioChunkReceived;
            _apiService.OnStreamFinished += HandleStreamFinished;
            _apiService.OnStreamError += HandleStreamError;

            InitializeCollectionsAndCommands();
            LoadInitialData();
        }
        
        private void InitializeCollectionsAndCommands()
        {
            AllTracks = new ObservableCollection<TrackDto>();
            FavoriteTracks = new ObservableCollection<TrackDto>();
            UserPlaylists = new ObservableCollection<PlaylistDto>();
            
            NowPlayingText = "Ничего не воспроизводится";
            
            ShowAllTracksCommand = new RelayCommand(async _ => { await LoadAllTracksAsync(); SetView(typeof(AllTracksView)); });
            // ShowSearchCommand = new RelayCommand(_ => SetView(typeof(SearchView))); // <-- ЗАКОММЕНТИРОВАТЬ ЭТУ СТРОКУ
            ShowFavoritesCommand = new RelayCommand(async _ => { await LoadFavoriteTracksAsync(); SetView(typeof(FavoritesView)); });
            ShowProfileCommand = new RelayCommand(_ => SetView(typeof(ProfileView)));
            ShowUploadTrackViewCommand = new RelayCommand(_ => SetView(typeof(UploadTrackView)));
            
            PlayCommand = new RelayCommand(TogglePlayPause, CanPlayPause);
            ToggleFavoriteCommand = new RelayCommand(async param => await ToggleFavoriteTrack(param), CanToggleFavoriteTrack);
            DownloadCommand = new RelayCommand(async param => await DownloadTrack(param), CanDownloadTrack);
            SelectTrackCommand = new RelayCommand(SelectTrackAction);
            SearchCommand = new RelayCommand(async _ => await ExecuteSearchAsync());
            DeleteTrackCommand = new RelayCommand(async param => await ExecuteDeleteTrackAsync(param), param => CanExecuteDeleteTrack(param));
            ShuffleCommand = new RelayCommand(_ => ShowNotImplementedMessage("Перемешивание"));
            PreviousTrackCommand = new RelayCommand(_ => PreviousTrack(), _ => AllTracks.Any() && SelectedTrack != null);
            NextTrackCommand = new RelayCommand(_ => NextTrack(), _ => AllTracks.Any() && SelectedTrack != null);
            RepeatCommand = new RelayCommand(_ => ShowNotImplementedMessage("Повтор трека/плейлиста"));
            CreatePlaylistCommand = new RelayCommand(CreatePlaylistAction);
            ShowLikedSongsCommand = new RelayCommand(async _ => { await LoadFavoriteTracksAsync(); /* SetView(typeof(FavoritesView)); */ });
            ShowRadioCommand = new RelayCommand(_ => ShowNotImplementedMessage("Радио"));
            OpenPlaylistCommand = new RelayCommand(OpenPlaylistAction, CanOpenPlaylist);
            ShowLyricsCommand = new RelayCommand(_ => ShowNotImplementedMessage("Текст песни"));
            ShowQueueCommand = new RelayCommand(_ => ShowNotImplementedMessage("Очередь воспроизведения"));
            VolumeCommand = new RelayCommand(param => { if (double.TryParse(param?.ToString(), out double vol)) Volume = vol; });
        }
        
        private void LoadInitialData()
        {
            LoadAllTracksAsync(); // Загружаем все треки при старте
            LoadFavoriteTracksAsync(); // Загружаем избранные треки при старте
            LoadUserPlaylistsAsync(); // Загружаем плейлисты пользователя
            
            // Устанавливаем начальное представление
            SetView(typeof(AllTracksView)); 
        }

        // Обработчики событий от ApiService для стриминга
        private Task HandleTrackInfoReceived(TrackDto trackInfo)
        {
            _streamingTrackInfo = trackInfo;
            _audioBuffer.Clear();
            IsStreamingActive = true;
            IsBuffering = true;
            StreamErrorMessage = string.Empty;
            NowPlayingText = $"Загрузка: {trackInfo.Title} - {trackInfo.Artist}";
            CurrentAudioStream = null; // Очищаем предыдущий стрим
            OnPropertyChanged(nameof(NowPlayingText)); // Уведомить UI
            return Task.CompletedTask;
        }

        private Task HandleAudioChunkReceived(byte[] chunk)
        {
            if (IsStreamingActive)
            {
                _audioBuffer.AddRange(chunk);
            }
            return Task.CompletedTask;
        }

        private Task HandleStreamFinished()
        {
            if (IsStreamingActive)
            {
                IsBuffering = false;
                CurrentAudioStream = new MemoryStream(_audioBuffer.ToArray());
                NowPlayingText = $"Готово к воспр.: {_streamingTrackInfo?.Title}"; 
                OnPropertyChanged(nameof(NowPlayingText));
            }
            return Task.CompletedTask;
        }

        private Task HandleStreamError(string errorMessage)
        {
            IsStreamingActive = false;
            IsBuffering = false;
            StreamErrorMessage = errorMessage;
            NowPlayingText = $"Ошибка стриминга: {errorMessage}";
            CurrentAudioStream = null;
            OnPropertyChanged(nameof(NowPlayingText));
            OnPropertyChanged(nameof(StreamErrorMessage));
            Application.Current.Dispatcher.Invoke(() => 
                MessageBox.Show($"Ошибка стриминга: {errorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error));
            return Task.CompletedTask;
        }

        private bool CanPlayPause(object parameter)
        {
            return (SelectedTrack != null && string.IsNullOrEmpty(StreamErrorMessage)) || IsPlaying;
        }

        private async void TogglePlayPause(object parameter)
        {
            if (IsPlaying) // Если уже играет, то ставим на паузу
            {
                IsPlaying = false;
                NowPlayingText = SelectedTrack != null ? $"Пауза: {SelectedTrack.Title}" : "Пауза";
            }
            else // Если не играет (стоял на паузе или это первый запуск)
            {
                if (SelectedTrack == null) // Повторная проверка на случай, если он стал null асинхронно
                {
                    NowPlayingText = "Трек не выбран (ошибка)"; 
                    IsPlaying = false;
                    return;
                }

                if (_currentlyLoadedTrackId == SelectedTrack.Id && !string.IsNullOrEmpty(CurrentAudioFilePath) && File.Exists(CurrentAudioFilePath))
                {
                    IsPlaying = true;
                    NowPlayingText = $"Воспроизводится: {SelectedTrack.Title}";
                }
                else 
                {
                    if (SelectedTrack == null) 
                    {
                        NowPlayingText = "Ошибка: Трек стал null перед загрузкой.";
                        IsPlaying = false;
                        IsLoading = false; 
                        return;
                    }

                    IsLoading = true;
                    Stream stream = null;
                    string oldTempFile = CurrentAudioFilePath; 

                    try
                    {
                        // Перед использованием SelectedTrack, убедимся что он не null
                        if (SelectedTrack == null) 
                        {
                             throw new InvalidOperationException("SelectedTrack is null before API call in TogglePlayPause.");
                        }
                        NowPlayingText = $"Загрузка: {SelectedTrack.Title}";
                        stream = await _apiService.GetTrackStreamAsync(SelectedTrack.Id);

                        if (stream == null || stream == Stream.Null || !stream.CanRead)
                        {
                            throw new Exception("Не удалось получить поток для трека от сервера.");
                        }
                        
                        if (SelectedTrack == null) 
                        { // Еще одна проверка перед использованием SelectedTrack.Id
                             throw new InvalidOperationException("SelectedTrack is null before Path.Combine in TogglePlayPause.");
                        }
                        string tempFile = Path.Combine(Path.GetTempPath(), $"music_client_track_{SelectedTrack.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}.mp3");
                        
                        using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(fs);
                        }
                        CurrentAudioFilePath = tempFile; // Устанавливаем путь к НОВОМУ файлу
                        _currentlyLoadedTrackId = SelectedTrack.Id; // Запоминаем, что этот трек загружен
                        
                        IsPlaying = true; // Это вызовет Open() и Play() в MainWindow.xaml.cs для нового файла
                        NowPlayingText = $"Воспроизводится: {SelectedTrack.Title}";

                        // Попытка удалить старый временный файл, если он был и отличается от нового
                        if (!string.IsNullOrEmpty(oldTempFile) && oldTempFile != CurrentAudioFilePath && File.Exists(oldTempFile))
                        {
                            try { File.Delete(oldTempFile); } catch (IOException ex) { Console.WriteLine($"Не удалось удалить старый временный файл {oldTempFile}: {ex.Message}"); }
                        }
                    }
                    catch (Exception ex)
                    {
                        NowPlayingText = $"Ошибка загрузки: {ex.Message}";
                        IsPlaying = false;
                        CurrentAudioFilePath = null; // Сбрасываем путь, так как загрузка не удалась
                        _currentlyLoadedTrackId = null; // Сбрасываем ID
                        MessageBox.Show($"Не удалось загрузить трек: {ex.Message}", "Ошибка воспроизведения", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        stream?.Dispose();
                        IsLoading = false;
                    }
                }
            }
        }
        
        private async Task LoadAllTracksAsync()
        {
            IsLoading = true;
            try
            {
                var tracksFromServer = await _apiService.GetAllTracksAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    int? playingTrackIdPriorToClear = null;
                    bool wasPlayingPriorToClear = IsPlaying && _selectedTrack != null;
                    if (wasPlayingPriorToClear)
                    {
                        playingTrackIdPriorToClear = _selectedTrack.Id;
                    }

                    AllTracks.Clear();
                    if (tracksFromServer != null)
                    {
                        foreach (var track in tracksFromServer) AllTracks.Add(track);
                    }

                    if (wasPlayingPriorToClear && playingTrackIdPriorToClear.HasValue)
                    {
                        var trackInstanceInNewList = AllTracks.FirstOrDefault(t => t.Id == playingTrackIdPriorToClear.Value);
                        if (trackInstanceInNewList != null)
                        {
                            if (_selectedTrack == null || !ReferenceEquals(_selectedTrack, trackInstanceInNewList))
                            {
                               if (_selectedTrack != null) _selectedTrack.IsCurrent = false;
                               _selectedTrack = trackInstanceInNewList;
                               _selectedTrack.IsCurrent = true;
                                OnPropertyChanged(nameof(SelectedTrack));
                            }
                            // IsPlaying и NowPlayingText не меняем
                        }
                        else
                        {
                            IsPlaying = false;
                            SelectedTrack = null; 
                        }
                    }
                    else if (_selectedTrack != null) 
                    {
                        var previouslySelected = AllTracks.FirstOrDefault(t => t.Id == _selectedTrack.Id);
                        if (previouslySelected != null) {
                            if (!ReferenceEquals(_selectedTrack, previouslySelected)){
                                if (_selectedTrack != null) _selectedTrack.IsCurrent = false;
                                _selectedTrack = previouslySelected;
                                 _selectedTrack.IsCurrent = true;
                                OnPropertyChanged(nameof(SelectedTrack));
                                NowPlayingText = $"Выбран: {_selectedTrack.Title} - {_selectedTrack.Artist}";
                            }
                        } else {
                            SelectedTrack = null; 
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadAllTracksAsync] Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadFavoriteTracksAsync()
        {
            IsLoading = true;
            try
            {
                var favTracksFromServer = await _apiService.GetLikedTracksAsync(); // Получаем актуальный список с сервера

                Application.Current.Dispatcher.Invoke(() =>
                {
                    int? currentPlayingTrackId = null;
                    bool trackWasPlaying = IsPlaying && _selectedTrack != null;

                    if (trackWasPlaying)
                    {
                        currentPlayingTrackId = _selectedTrack.Id;
                        Debug.WriteLine($"[LoadFavoriteTracksAsync] Track was playing: ID {currentPlayingTrackId}");
                    }

                    FavoriteTracks.Clear();
                    if (favTracksFromServer != null) 
                    {
                        foreach (var track in favTracksFromServer) FavoriteTracks.Add(track);
                    }
                    Debug.WriteLine($"[LoadFavoriteTracksAsync] FavoriteTracks loaded. Count: {FavoriteTracks.Count}");

                    if (trackWasPlaying && currentPlayingTrackId.HasValue)
                    {
                        var playingTrackInNewFavorites = FavoriteTracks.FirstOrDefault(t => t.Id == currentPlayingTrackId.Value);

                        if (playingTrackInNewFavorites != null)
                        {
                            Debug.WriteLine($"[LoadFavoriteTracksAsync] Playing track ID {currentPlayingTrackId} FOUND in new FavoriteTracks.");
                            if (_selectedTrack == null || !ReferenceEquals(_selectedTrack, playingTrackInNewFavorites))
                            {
                                if (_selectedTrack != null) _selectedTrack.IsCurrent = false;
                                _selectedTrack = playingTrackInNewFavorites;
                                _selectedTrack.IsCurrent = true;
                                OnPropertyChanged(nameof(SelectedTrack));
                            }
                        }
                        else 
                        {
                            Debug.WriteLine($"[LoadFavoriteTracksAsync] Playing track ID {currentPlayingTrackId} NOT found in new FavoriteTracks. Checking AllTracks.");
                            Debug.WriteLine($"[LoadFavoriteTracksAsync] AllTracks current count: {AllTracks.Count}. First few IDs: {string.Join(", ", AllTracks.Take(5).Select(t=>t.Id))}");

                            var playingTrackInAllTracks = AllTracks.FirstOrDefault(t => t.Id == currentPlayingTrackId.Value);
                            if (playingTrackInAllTracks != null)
                            {
                                Debug.WriteLine($"[LoadFavoriteTracksAsync] Playing track ID {currentPlayingTrackId} FOUND in AllTracks. Switching _selectedTrack instance.");
                                if (_selectedTrack == null || !ReferenceEquals(_selectedTrack, playingTrackInAllTracks))
                                {
                                    if (_selectedTrack != null) _selectedTrack.IsCurrent = false;
                                    _selectedTrack = playingTrackInAllTracks;
                                    _selectedTrack.IsCurrent = true;
                                    OnPropertyChanged(nameof(SelectedTrack));
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"[LoadFavoriteTracksAsync] Playing track ID {currentPlayingTrackId} NOT found in AllTracks EITHER. Stopping playback.");
                                IsPlaying = false;
                                SelectedTrack = null; 
                            }
                        }
                    }
                    else if (_selectedTrack != null && !trackWasPlaying) 
                    {
                        var previouslySelected = FavoriteTracks.FirstOrDefault(t => t.Id == _selectedTrack.Id);
                        if (previouslySelected != null)
                        {
                            if (!ReferenceEquals(_selectedTrack, previouslySelected))
                            {
                                if (_selectedTrack != null) _selectedTrack.IsCurrent = false;
                                _selectedTrack = previouslySelected;
                                _selectedTrack.IsCurrent = true;
                                OnPropertyChanged(nameof(SelectedTrack));
                                NowPlayingText = $"Выбран: {_selectedTrack.Title} - {_selectedTrack.Artist}";
                            }
                        }
                        else
                        {
                            SelectedTrack = null; 
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки избранных треков: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void LoadUserPlaylistsAsync()
            {
                IsLoading = true;
            UserPlaylists.Clear();
                try
                {
                var playlists = await _apiService.GetUserPlaylistsAsync();
                if (playlists != null)
                    {
                    foreach (var playlistDto in playlists)
                    {
                        UserPlaylists.Add(playlistDto);
                    }
                    }
                    else
                    {
                    MessageBox.Show("Плейлисты пользователя не были загружены с сервера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Нет соединения с сервером или сервер не отвечает.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
                    {
                MessageBox.Show("Время ожидания ответа от сервера истекло.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                MessageBox.Show($"Неизвестная ошибка при загрузке плейлистов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
            }
        }
        
        private bool CanToggleFavoriteTrack(object parameter)
        {
            return parameter is TrackDto track && track != null;
        }
        
        private async Task ToggleFavoriteTrack(object parameter)
        {
            if (parameter is TrackDto track)
            {
                bool originalIsLiked = track.IsLiked; // Сохраняем текущее состояние
                // Оптимистично обновляем UI
                // track.IsLiked = !track.IsLiked; 
                // Не будем делать оптимистичное обновление, дождемся ответа сервера, 
                // так как сервер все равно пришлет актуальное состояние IsLiked при следующем GetAllTracks
                // или можно обновить после успешного ответа.

                    bool success;
                if (originalIsLiked) // Если трек был лайкнут, то анлайкаем
                    {
                    success = await _apiService.UnlikeTrackAsync(track.Id);
                    if (success)
                    {
                        track.IsLiked = false; // Обновляем состояние после успешного ответа
                        // Удаляем из локальной коллекции FavoriteTracks, если он там был
                        var trackInFavorites = FavoriteTracks.FirstOrDefault(t => t.Id == track.Id);
                        if (trackInFavorites != null)
                        {
                            FavoriteTracks.Remove(trackInFavorites);
                        }
                    }
                        }
                else // Если трек не был лайкнут, то лайкаем
                {
                    success = await _apiService.LikeTrackAsync(track.Id);
                    if (success)
                        {
                        track.IsLiked = true; // Обновляем состояние после успешного ответа
                        // Добавляем в локальную коллекцию FavoriteTracks, если его там еще нет
                        if (!FavoriteTracks.Any(t => t.Id == track.Id))
                        {
                            FavoriteTracks.Add(track);
                        }
                    }
                }

                if (!success)
                {
                    // Можно показать сообщение об ошибке, если нужно
                    MessageBox.Show("Не удалось обновить статус избранного.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    // track.IsLiked = originalIsLiked; // Вернуть UI к исходному состоянию, если операция не удалась
                    // Но мы уже решили положиться на серверное состояние при следующей загрузке треков.
                    // Либо можно принудительно перезагрузить все треки, чтобы получить 100% актуальное состояние.
                }
                 // После лайка/анлайка, если открыт список избранного, его стоит обновить.
                 // Можно, например, снова вызвать LoadFavoriteTracksAsync(), если текущее view - это избранное.
                 // Или если используется ShowFavoritesCommand, он должен сам загружать актуальные данные.
            }
        }
        
        private async void CreatePlaylistAction(object obj)
        {
            ShowNotImplementedMessage("Create Playlist (InputDialogWindow is missing)");
        }
        
        private bool CanOpenPlaylist(object parameter)
        {
            return parameter is PlaylistDto;
        }
        
        private async void OpenPlaylistAction(object parameter)
        {
            if (parameter is PlaylistDto playlistToOpen)
            {
                IsLoading = true;
                try
                {
                    var playlistWithTracks = await _apiService.GetPlaylistAsync(playlistToOpen.Id);
                    if (playlistWithTracks?.Tracks != null)
                    {
                        var existingPlaylistInVM = UserPlaylists.FirstOrDefault(p => p.Id == playlistToOpen.Id);
                        if (existingPlaylistInVM != null)
                        {
                            existingPlaylistInVM.Tracks = new ObservableCollection<TrackDto>(playlistWithTracks.Tracks);
                            OnPropertyChanged(nameof(UserPlaylists));
                        }
                        System.Windows.MessageBox.Show($"Открыт плейлист '{playlistWithTracks.Name}'. Количество треков: {playlistWithTracks.Tracks.Count()}", "Плейлист", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Не удалось загрузить треки для плейлиста '{playlistToOpen.Name}'.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при открытии плейлиста: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
        
        private void SetView(Type viewType)
        {
            if (viewType == typeof(AllTracksView))
            {
                CurrentView = new AllTracksView { DataContext = this };
            }
            else if (viewType == typeof(FavoritesView))
            {
                CurrentView = new FavoritesView { DataContext = this };
            }
            else if (viewType == typeof(ProfileView))
            {
                 // Предполагаем, что ProfileViewModel создается внутри ProfileView или не требует MainViewModel в качестве DataContext
                CurrentView = new ProfileView { DataContext = new ProfileViewModel(_currentUser, _apiService) };
            }
            else if (viewType == typeof(UploadTrackView))
            {
                CurrentView = new UploadTrackView { DataContext = new UploadTrackViewModel(_apiService, _currentUser, this.LoadAllTracksAsync) };
            }
            // else if (viewType == typeof(SearchView))
            // {
            //     CurrentView = new SearchView { DataContext = this }; // Если SearchView использует MainViewModel
            // }
            else
            {
                // Обработка неизвестного типа View или установка по умолчанию
                CurrentView = new AllTracksView { DataContext = this };
            }
        }
        
        private void ShowNotImplementedMessage(string featureName)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show($"Функция \"{featureName}\" еще не реализована.", "В разработке", MessageBoxButton.OK, MessageBoxImage.Information));
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async ValueTask DisposeAsync()
        {
            if (_apiService != null)
            {
                _apiService.OnTrackInfoReceived -= HandleTrackInfoReceived;
                _apiService.OnAudioChunkReceived -= HandleAudioChunkReceived;
                _apiService.OnStreamFinished -= HandleStreamFinished;
                _apiService.OnStreamError -= HandleStreamError;
            }
            _currentAudioStream?.Dispose();
            await Task.CompletedTask;
        }

        private bool CanDownloadTrack(object parameter)
        {
            return parameter is TrackDto;
        }
        private async Task DownloadTrack(object parameter)
        {
            if (parameter is TrackDto track)
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = track.Title + ".mp3",
                    Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        using (var stream = await _apiService.GetTrackStreamAsync(track.Id))
                        using (var fs = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(fs);
                        }
                        MessageBox.Show($"Трек '{track.Title}' успешно скачан!", "Скачивание", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при скачивании: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void PreviousTrack()
        {
            if (AllTracks == null || AllTracks.Count == 0) return;
            if (SelectedTrack == null)
            {
                SelectedTrack = AllTracks.First();
                return;
            }
            var idx = AllTracks.IndexOf(SelectedTrack);
            if (idx > 0)
                SelectedTrack = AllTracks[idx - 1];
            else
                SelectedTrack = AllTracks.Last();
        }

        private void NextTrack()
        {
            if (AllTracks == null || AllTracks.Count == 0) return;
            if (SelectedTrack == null)
            {
                SelectedTrack = AllTracks.First();
                return;
            }
            var idx = AllTracks.IndexOf(SelectedTrack);
            if (idx < AllTracks.Count - 1)
                SelectedTrack = AllTracks[idx + 1];
            else
                SelectedTrack = AllTracks.First();
        }

        public void Seek(double newPositionInSeconds)
        {
            if (TotalDuration > 0)
            {
                CurrentPosition = Math.Max(0, Math.Min(newPositionInSeconds, TotalDuration));
            }
        }

        private async Task ExecuteSearchAsync()
        {
            IsLoading = true;
            AllTracks.Clear();
            try
            {
                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    await LoadAllTracksAsync(); 
                }
                else
                {
                    var searchResults = await _apiService.SearchTracksAsync(SearchQuery);
                    if (searchResults != null)
                    {
                        foreach (var track in searchResults)
                        {
                            AllTracks.Add(track);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SelectTrackAction(object parameter)
        {
            if (parameter is TrackDto track)
            {
                if (SelectedTrack != null && SelectedTrack.Id == track.Id) 
                {
                    TogglePlayPause(null); 
                }
                else 
                {
                    SelectedTrack = track; 
                    
                    if (SelectedTrack != null)
                    {
                        IsPlaying = false; 
                        _currentlyLoadedTrackId = null; 
                        CurrentPosition = 0;            
                        TogglePlayPause(null);          
                    }
                }
            }
        }

        private void ShowProfileView()
        {
            var profileViewModel = new ProfileViewModel(_currentUser, _apiService);
            CurrentView = new ProfileView { DataContext = profileViewModel };
        }

        private async Task ExecuteDeleteTrackAsync(object parameter)
        {
            if (parameter is TrackDto trackToDelete)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить трек '{trackToDelete.Title} - {trackToDelete.Artist}'? Это действие нельзя будет отменить.", 
                                             "Подтверждение удаления", 
                                             MessageBoxButton.YesNo, 
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    try
                    {
                        var (success, message) = await _apiService.DeleteTrackAsync(trackToDelete.Id);
                        if (success)
                        {
                            AllTracks.Remove(trackToDelete);
                            FavoriteTracks.Remove(FavoriteTracks.FirstOrDefault(t => t.Id == trackToDelete.Id));
                            if (SelectedTrack?.Id == trackToDelete.Id)
                            {
                                SelectedTrack = null;
                                IsPlaying = false;
                                CurrentAudioStream?.Dispose();
                                CurrentAudioStream = null;
                                CurrentAudioFilePath = null;
                            }
                            MessageBox.Show(message, "Удаление успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка при удалении трека: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }

        private bool CanExecuteDeleteTrack(object parameter)
        {
            return parameter is TrackDto;
        }
    }
    
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
