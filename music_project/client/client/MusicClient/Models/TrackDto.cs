using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicClient.Models
{
    public class TrackDto : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public double Duration { get; set; } // В секундах
        public string FilePath { get; set; } // URL или путь к файлу, если нужно

        private bool _isLiked;
        public bool IsLiked
        {
            get => _isLiked;
            set
            {
                if (_isLiked != value)
                {
                    _isLiked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCurrent;
        public bool IsCurrent // Для выделения текущего трека
        {
            get => _isCurrent;
            set
            {
                if (_isCurrent != value)
                {
                    _isCurrent = value;
                    OnPropertyChanged();
                }
            }
        }

        public double CurrentPositionMs { get; set; } // Для прогресс-бара

        public string DurationFormatted => TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj is TrackDto otherTrack)
            {
                return Id == otherTrack.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
} 