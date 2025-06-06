using MusicClient.Models;
using MusicClient.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicClient.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _nickname;
        private string _email;

        public string Nickname
        {
            get => _nickname;
            set
            {
                _nickname = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public ProfileViewModel(AuthResponseDto currentUser, ApiService apiService)
        {
            _apiService = apiService;

            if (currentUser != null)
            {
                Nickname = currentUser.Nickname;
                Email = currentUser.Email;
            }
            else
            {
                Nickname = "N/A";
                Email = "N/A";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 