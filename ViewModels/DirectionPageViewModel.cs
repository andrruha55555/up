using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.ViewModels
{
    public class DirectionPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Direction> _directionList;
        private Direction _selectedDirection;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Direction> DirectionList
        {
            get => _directionList;
            set
            {
                _directionList = value;
                OnPropertyChanged();
            }
        }

        public Direction SelectedDirection
        {
            get => _selectedDirection;
            set
            {
                _selectedDirection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDirectionSelected));
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

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterDirections();
            }
        }

        public bool IsDirectionSelected => SelectedDirection != null;

        public ObservableCollection<Direction> FilteredDirectionList { get; set; }

        public DirectionPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            DirectionList = new ObservableCollection<Direction>();
            FilteredDirectionList = new ObservableCollection<Direction>();
        }

        public async Task LoadDirectionsAsync()
        {
            IsLoading = true;
            try
            {
                var directions = await _cacheService.GetOrSetAsync("directions_page_list",
                    async () => await _apiService.GetListAsync<Direction>("DirectionsController"));

                DirectionList.Clear();
                if (directions != null)
                {
                    foreach (var item in directions)
                    {
                        DirectionList.Add(item);
                    }
                }

                FilterDirections();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки направлений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterDirections()
        {
            FilteredDirectionList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in DirectionList)
                {
                    FilteredDirectionList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = DirectionList.Where(d =>
                    (d.name?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredDirectionList.Add(item);
                }
            }
        }

        public async Task<bool> AddDirectionAsync(Direction direction)
        {
            try
            {
                var success = await _apiService.AddItemAsync("DirectionsController", direction);
                if (success)
                {
                    _cacheService.Remove("directions_page_list");
                    await LoadDirectionsAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateDirectionAsync(int id, Direction direction)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("DirectionsController", id, direction);
                if (success)
                {
                    _cacheService.Remove("directions_page_list");
                    await LoadDirectionsAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteDirectionAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это направление?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("DirectionsController", id);
                if (success)
                {
                    _cacheService.Remove("directions_page_list");
                    await LoadDirectionsAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}