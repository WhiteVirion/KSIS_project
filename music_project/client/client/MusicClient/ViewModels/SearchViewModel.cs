using MusicClient.Models;
using MusicClient.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System;

namespace MusicClient.ViewModels
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _searchQuery;
        private ObservableCollection<TrackDto> _searchResults;
        private bool _isLoading;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TrackDto> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand PlayTrackCommand { get; }

        public event Action<TrackDto> PlayTrackRequested;

        public SearchViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SearchResults = new ObservableCollection<TrackDto>();
            SearchCommand = new RelayCommand(async (_) => await PerformSearch(), (_) => !IsLoading && !string.IsNullOrWhiteSpace(SearchQuery));
            PlayTrackCommand = new RelayCommand(trackAsObject => 
            {
                if (trackAsObject is TrackDto selectedTrackDto)
                {
                    PlayTrackRequested?.Invoke(selectedTrackDto);
                }
            });
        }

        private async Task PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                return;
            }

            IsLoading = true;
            try
            {
                var results = await _apiService.SearchTracksAsync(SearchQuery);
                SearchResults.Clear();
                if (results != null)
                {
                    foreach (var trackDto in results) 
                    {
                        SearchResults.Add(trackDto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
                SearchResults.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 