using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MusicClient.Models;
using MusicClient.Services;

namespace MusicClient.ViewModels
{
    public class UploadTrackViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly AuthResponseDto _currentUser;
        private readonly Func<Task> _onUploadSuccessAction;

        private string _selectedFilePath;
        private string _title;
        private string _artist;
        private bool _isUploading;
        private string _statusMessage;
        private double _uploadProgress;

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set { _selectedFilePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanUpload)); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }

        public bool IsUploading
        {
            get => _isUploading;
            set { _isUploading = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSelectFile)); OnPropertyChanged(nameof(CanUpload)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }
        
        public double UploadProgress
        {
            get => _uploadProgress;
            set { _uploadProgress = value; OnPropertyChanged(); }
        }

        public ICommand SelectFileCommand { get; }
        public ICommand UploadCommand { get; }

        public bool CanSelectFile => !IsUploading;
        public bool CanUpload => !string.IsNullOrEmpty(SelectedFilePath) && !IsUploading;

        public UploadTrackViewModel(ApiService apiService, AuthResponseDto currentUser, Func<Task> onUploadSuccessAction)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _onUploadSuccessAction = onUploadSuccessAction;

            SelectFileCommand = new RelayCommand(SelectFileAction, _ => CanSelectFile);
            UploadCommand = new RelayCommand(async _ => await UploadFileActionAsync(), _ => CanUpload);
        }

        private void SelectFileAction(object parameter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Audio Files (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|All files (*.*)|*.*",
                Title = "Выберите аудиофайл для загрузки"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
                // Попробуем извлечь Title и Artist из имени файла
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                var parts = fileNameWithoutExtension.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    Artist = parts[0].Trim();
                    Title = parts[1].Trim();
                }
                else
                {
                    Title = fileNameWithoutExtension.Trim();
                    Artist = string.Empty;
                }
                StatusMessage = $"Выбран файл: {Path.GetFileName(SelectedFilePath)}";
            }
        }

        private async Task UploadFileActionAsync()
        {
            if (!CanUpload) return;

            IsUploading = true;
            StatusMessage = "Загрузка началась...";
            UploadProgress = 0;

            FileStream fileStream = null;
            try
            {
                string fileName = Path.GetFileName(SelectedFilePath);
                // Если Title или Artist не были введены вручную, используем те, что извлекли из имени файла,
                // или оставляем пустыми, чтобы сервер сам попытался их определить из тегов/имени.
                string currentTitle = string.IsNullOrWhiteSpace(Title) ? Path.GetFileNameWithoutExtension(fileName) : Title;
                string currentArtist = Artist; // Artist может быть пустым

                fileStream = new FileStream(SelectedFilePath, FileMode.Open, FileAccess.Read);
                
                string contentType = "application/octet-stream"; // По умолчанию
                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (extension == ".mp3") contentType = "audio/mpeg";
                else if (extension == ".wav") contentType = "audio/wav";
                else if (extension == ".ogg") contentType = "audio/ogg";

                // В ApiService.UploadTrackAsync можно добавить параметр для отслеживания прогресса, если он это поддерживает.
                // Пока что просто имитируем прогресс или оставляем его на 0 до завершения.
                // Для реального прогресса нужно будет модифицировать ApiService и сервер.

                var uploadedTrack = await _apiService.UploadTrackAsync(fileStream, fileName, currentTitle, currentArtist, "", "", contentType);

                if (uploadedTrack != null)
                {
                    StatusMessage = $"Трек '{uploadedTrack.Title}' успешно загружен!";
                    UploadProgress = 100;
                    _onUploadSuccessAction?.Invoke();
                    // Очистить поля после успешной загрузки
                    SelectedFilePath = null;
                    Title = null;
                    Artist = null;
                }
                else
                {
                    StatusMessage = "Не удалось загрузить трек. Ответ от сервера не получен или некорректен.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при загрузке трека: {ex.Message}";
                UploadProgress = 0;
            }
            finally
            {
                fileStream?.Dispose();
                IsUploading = false;
                // Чтобы кнопки обновили свое состояние CanExecute - это больше не нужно, WPF сделает это автоматически
                // Application.Current.Dispatcher.Invoke(() => 
                // {
                //    ((RelayCommand)SelectFileCommand).RaiseCanExecuteChanged();
                //    ((RelayCommand)UploadCommand).RaiseCanExecuteChanged();
                // });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 