using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicClient.Models
{
    public class Track : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public string UploadDate { get; set; }

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Duration { get; set; }
        
        public string DisplayName => $"{Title} - {Artist}";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
