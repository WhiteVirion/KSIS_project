using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MusicClient.ViewModels;
using MusicClient.Services;
using MusicClient.Models;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MusicClient
{
    public partial class MainWindow : Window
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly ApiService _apiService;
        private readonly AuthResponseDto _currentUser;
        private readonly MainViewModel _mainViewModel;
        private DispatcherTimer _timer;
        private bool _isDraggingSlider = false;

        // Конструктор для XAML-дизайнера и парсера
        public MainWindow()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // Можно установить DataContext на "дизайнерскую" ViewModel, если необходимо.
                // DataContext = new DesignTimeMainViewModel(); 
            }
        }
        
        public MainWindow(ApiService apiService, AuthResponseDto currentUser)
        {
            InitializeComponent(); // Убедимся, что этот вызов есть
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            Title = $"Music Client - Пользователь: {_currentUser.Nickname}";

            _mainViewModel = new MainViewModel(_apiService, _currentUser);
            DataContext = _mainViewModel;

            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            
            Loaded += MainWindow_Loaded;

            InitializeSignalRAsync();

            // Обработчики для кастомного заголовка окна должны быть здесь,
            // так как элементы управления (MinimizeButton и т.д.) инициализируются в InitializeComponent()
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Application.Current.Shutdown();
            TitleBar.MouseLeftButtonDown += (s, e) => { if (e.ClickCount == 2) MaximizeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); else DragMove(); };

            _mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentAudioFilePath))
                {
                    if (!string.IsNullOrEmpty(_mainViewModel.CurrentAudioFilePath))
                    {
                        _mediaPlayer.Open(new Uri(_mainViewModel.CurrentAudioFilePath));
                        _mediaPlayer.Volume = _mainViewModel.Volume; // Устанавливаем громкость при открытии нового файла
                        if (_mainViewModel.IsPlaying) // Если новый трек должен сразу играть
                        {
                            _mediaPlayer.Play();
                            // Если для нового трека была сохранена позиция (маловероятно, но для общности)
                            if (_mainViewModel.CurrentPosition > 0 && _mediaPlayer.NaturalDuration.HasTimeSpan && _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds > _mainViewModel.CurrentPosition)
                            {
                                _mediaPlayer.Position = TimeSpan.FromSeconds(_mainViewModel.CurrentPosition);
                            }
                        }
                        else // Если новый трек не должен играть сразу, но позиция могла быть установлена
                        {
                             // Позиция должна быть установлена ДО Play(), если мы не играем сразу.
                             // Но MediaPlayer может требовать Open перед установкой Position.
                             // Если мы не вызываем Play(), то установка Position здесь может быть не нужна
                             // или должна происходить после Open, но до фактического Play.
                             // Пока оставляем логику как есть для случая, когда IsPlaying false при смене трека.
                        }
                    }
                    else
                    {
                        _mediaPlayer.Close(); // Закрываем, если путь стал null или пустым
                    }
                }
                else if (e.PropertyName == nameof(MainViewModel.IsPlaying))
                {
                    if (_mainViewModel.IsPlaying)
                    {
                        if (_mediaPlayer.Source != null) // Убедимся, что источник уже загружен
                        {
                             _mediaPlayer.Play();
                        }
                        // Если источник null, но CurrentAudioFilePath есть, значит, трек еще не был открыт.
                        // Это может произойти, если IsPlaying устанавливается в true до того, как CurrentAudioFilePath обработан.
                        // В этом случае блок выше (для CurrentAudioFilePath) должен корректно его открыть и запустить.
                    }
                    else
                    {
                        if (_mediaPlayer.CanPause) // Проверяем, можно ли поставить на паузу
                        {
                            _mediaPlayer.Pause();
                        }
                    }
                }
                else if (e.PropertyName == nameof(MainViewModel.Volume))
                {
                    _mediaPlayer.Volume = _mainViewModel.Volume;
                }
                else if (e.PropertyName == nameof(MainViewModel.CurrentPosition))
                {
                    if (_mediaPlayer.Source != null && _mediaPlayer.NaturalDuration.HasTimeSpan && !_isDraggingSlider)
                    {
                        // Обновляем позицию плеера, только если она действительно отличается
                        // и если пользователь не перетаскивает слайдер в данный момент.
                        // Добавляем небольшую дельту для сравнения double, чтобы избежать циклов из-за погрешностей.
                        if (Math.Abs(_mediaPlayer.Position.TotalSeconds - _mainViewModel.CurrentPosition) > 0.1)
                        {
                             if (_mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds > _mainViewModel.CurrentPosition)
                             {
                                _mediaPlayer.Position = TimeSpan.FromSeconds(_mainViewModel.CurrentPosition);
                             }
                        }
                    }
                    else if (_mainViewModel.IsPlaying && !string.IsNullOrEmpty(_mainViewModel.CurrentAudioFilePath) && _mediaPlayer.Source == null)
                    {
                        // Если плеер должен играть, но источник не открыт (например, после Seek до первого Play)
                        _mediaPlayer.Open(new Uri(_mainViewModel.CurrentAudioFilePath));
                        _mediaPlayer.Volume = _mainViewModel.Volume;
                        _mediaPlayer.Play();
                        if (_mediaPlayer.NaturalDuration.HasTimeSpan && _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds > _mainViewModel.CurrentPosition)
                        {
                             _mediaPlayer.Position = TimeSpan.FromSeconds(_mainViewModel.CurrentPosition);
                        }
                    }
                }
            };

            // Таймер для обновления прогресса
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += PlayerTimer_Tick;
            _timer.Start();
        }
        
        private async void InitializeSignalRAsync()
        {
            if (_apiService != null)
            {
                await _apiService.InitializeHubConnectionAsync();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mainViewModel != null)
            {
                _mainViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
            else
            {
                MessageBox.Show("Ошибка: ViewModel не загружена.", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
        
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentAudioStream))
            {
                // Если изменился сам поток и мы должны играть, открываем его
                if (_mainViewModel.CurrentAudioStream != null && _mainViewModel.CurrentAudioStream.Length > 0 && _mainViewModel.IsPlaying)
                {
                    PlayAudioStream(_mainViewModel.CurrentAudioStream);
                }
                // Если CurrentAudioStream обнулен, возможно, стоит закрыть плеер, 
                // но только если он играл именно этот стрим. Пока оставляем это без изменений,
                // так как MainViewModel должен управлять, когда очищать плеер полностью.
            }
            else if (e.PropertyName == nameof(MainViewModel.IsPlaying))
            {
                // Этот обработчик IsPlaying в ViewModel_PropertyChanged должен срабатывать
                // преимущественно для сценариев, когда активен CurrentAudioStream.
                if (_mainViewModel.CurrentAudioStream != null && _mainViewModel.CurrentAudioStream.Length > 0)
                {
                    if (_mainViewModel.IsPlaying)
                    {
                        // Проверяем, является ли текущий источник в плеере темп-файлом (созданным PlayAudioStream)
                        // и может ли он быть возобновлен.
                        bool isPlayingFromStreamTempFile = _mediaPlayer.Source != null &&
                                                           _mediaPlayer.Source.IsFile &&
                                                           _mediaPlayer.Source.LocalPath.Contains(Path.GetTempPath());

                        if (isPlayingFromStreamTempFile &&
                            _mediaPlayer.CanPause && /* Убедимся, что его можно поставить на паузу/возобновить */
                            _mediaPlayer.NaturalDuration.HasTimeSpan && /* Убедимся, что длительность известна */
                            _mediaPlayer.Position < _mediaPlayer.NaturalDuration.TimeSpan) /* Убедимся, что не в конце */
                        {
                            _mediaPlayer.Play(); // Возобновить воспроизведение потока
                        }
                        else
                        {
                            // Если не можем возобновить текущий поток (например, другой источник или конец файла),
                            // или если это первая загрузка потока, то открываем его заново.
                            PlayAudioStream(_mainViewModel.CurrentAudioStream);
                        }
                    }
                    else // IsPlaying is false
                    {
                        // Если текущий источник - это темп-файл от нашего потока, ставим на паузу.
                        bool isPlayingFromStreamTempFile = _mediaPlayer.Source != null &&
                                                           _mediaPlayer.Source.IsFile &&
                                                           _mediaPlayer.Source.LocalPath.Contains(Path.GetTempPath());
                        if (isPlayingFromStreamTempFile && _mediaPlayer.CanPause)
                        {
                            _mediaPlayer.Pause();
                        }
                        // Если CurrentAudioStream активен, но плеер играет что-то другое, не трогаем (анонимный обработчик разберется).
                    }
                }
                // Если CurrentAudioStream == null, то изменения IsPlaying, вероятно, касаются CurrentAudioFilePath,
                // и они обрабатываются анонимным делегатом в конструкторе MainWindow.
            }
        }
        
        private void PlayAudioStream(MemoryStream audioStream)
        {
            if (audioStream == null || audioStream.Length == 0) 
            {
                _mainViewModel.IsPlaying = false;
                return;
            }

                try
                {
                    audioStream.Position = 0;
                
                    string tempFilePath = Path.GetTempFileName();

                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        audioStream.CopyTo(fileStream);
                    }
                    _mediaPlayer.Open(new Uri(tempFilePath));
                    _mediaPlayer.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка плеера", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (_mainViewModel.IsPlaying) _mainViewModel.IsPlaying = false;
                }
            }
        
        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            _mainViewModel.IsPlaying = false;
            _mediaPlayer.Close();
        }
        
        private void MediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
        {
            _mainViewModel.IsPlaying = false;
            MessageBox.Show($"Ошибка файла мультимедиа: {e.ErrorException.Message}", "Ошибка воспроизведения", MessageBoxButton.OK, MessageBoxImage.Error);
            _mediaPlayer.Close();
        }

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_mainViewModel != null)
            {
                await _mainViewModel.DisposeAsync();
            }
            if (_apiService != null)
            {
                await _apiService.DisposeAsync();
            }
            _mediaPlayer?.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _apiService?.Logout();

            // TokenStorage.ClearToken(); 

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            if (_mainViewModel != null)
            {
                 _mainViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            Loaded -= MainWindow_Loaded;
            if (_timer != null)
            {
                _timer.Tick -= PlayerTimer_Tick;
                _timer.Stop();
            }
            if(_mediaPlayer != null)
            {
                _mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                _mediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;
            }
            
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProgressBarTimer_Tick(object sender, EventArgs e)
        {
            
        }

        private void PlayerTimer_Tick(object? sender, EventArgs e)
        {
            if (_mediaPlayer.Source != null && _mediaPlayer.NaturalDuration.HasTimeSpan && !_isDraggingSlider)
            {
                if (_mainViewModel != null)
                {
                    _mainViewModel.TotalDuration = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    _mainViewModel.CurrentPosition = _mediaPlayer.Position.TotalSeconds;
                    _mainViewModel.CurrentTime = _mediaPlayer.Position.ToString("mm\\:ss");
                    _mainViewModel.TotalTime = _mediaPlayer.NaturalDuration.TimeSpan.ToString("mm\\:ss");
                }
            }
        }

        // Новые обработчики для ProgressSlider
        private void ProgressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isDraggingSlider = true;
            // Можно добавить логику, например, приостановить обновление слайдера из ViewModel
        }

        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isDraggingSlider = false;
            if (sender is Slider slider)
            {
                _mainViewModel?.Seek(slider.Value); // Используем существующий метод Seek во ViewModel
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isDraggingSlider && _mainViewModel != null && sender is Slider slider)
            {
                // Если изменение не вызвано перетаскиванием, и ViewModel существует,
                // то можно обновить позицию в ViewModel, хотя двусторонняя привязка должна это делать.
                // Если ViewModel обновляет слайдер, а слайдер обновляет ViewModel - может быть цикл.
                // _mainViewModel.CurrentPosition = slider.Value; // Осторожно с этим
            }
        }

        // Новый обработчик для VolumeSlider
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mainViewModel != null && sender is Slider slider)
            {
                _mainViewModel.Volume = slider.Value; // Громкость обычно можно сразу применять
            }
        }

       

      
    }
}
