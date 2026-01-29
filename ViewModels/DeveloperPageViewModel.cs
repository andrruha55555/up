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
    public class DeveloperPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Developer> _developerList;
        private Developer _selectedDeveloper;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Developer> DeveloperList
        {
            get => _developerList;
            set
            {
                _developerList = value;
                OnPropertyChanged();
            }
        }

        public Developer SelectedDeveloper
        {
            get => _selectedDeveloper;
            set
            {
                _selectedDeveloper = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDeveloperSelected));
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
                FilterDevelopers();
            }
        }

        public bool IsDeveloperSelected => SelectedDeveloper != null;

        public ObservableCollection<Developer> FilteredDeveloperList { get; set; }

        public DeveloperPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            DeveloperList = new ObservableCollection<Developer>();
            FilteredDeveloperList = new ObservableCollection<Developer>();
        }

        public async Task LoadDevelopersAsync()
        {
            IsLoading = true;
            try
            {
                var developers = await _cacheService.GetOrSetAsync("developers_page_list",
                    async () => await _apiService.GetListAsync<Developer>("DevelopersController"));

                DeveloperList.Clear();
                if (developers != null)
                {
                    foreach (var item in developers)
                    {
                        DeveloperList.Add(item);
                    }
                }

                FilterDevelopers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки разработчиков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterDevelopers()
        {
            FilteredDeveloperList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in DeveloperList)
                {
                    FilteredDeveloperList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = DeveloperList.Where(d =>
                    (d.Name?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredDeveloperList.Add(item);
                }
            }
        }

        public async Task<bool> AddDeveloperAsync(Developer developer)
        {
            try
            {
                var success = await _apiService.AddItemAsync("DevelopersController", developer);
                if (success)
                {
                    _cacheService.Remove("developers_page_list");
                    await LoadDevelopersAsync();
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

        public async Task<bool> UpdateDeveloperAsync(int id, Developer developer)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("DevelopersController", id, developer);
                if (success)
                {
                    _cacheService.Remove("developers_page_list");
                    await LoadDevelopersAsync();
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

        public async Task<bool> DeleteDeveloperAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого разработчика?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("DevelopersController", id);
                if (success)
                {
                    _cacheService.Remove("developers_page_list");
                    await LoadDevelopersAsync();
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