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
    public class SoftwarePageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Software> _softwareList;
        private Software _selectedSoftware;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Software> SoftwareList
        {
            get => _softwareList;
            set
            {
                _softwareList = value;
                OnPropertyChanged();
            }
        }

        public Software SelectedSoftware
        {
            get => _selectedSoftware;
            set
            {
                _selectedSoftware = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSoftwareSelected));
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
                FilterSoftware();
            }
        }

        public bool IsSoftwareSelected => SelectedSoftware != null;

        public ObservableCollection<Software> FilteredSoftwareList { get; set; }

        public SoftwarePageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            SoftwareList = new ObservableCollection<Software>();
            FilteredSoftwareList = new ObservableCollection<Software>();
        }

        public async Task LoadSoftwareAsync()
        {
            IsLoading = true;
            try
            {
                var software = await _cacheService.GetOrSetAsync("software_page_list",
                    async () => await _apiService.GetListAsync<Software>("SoftwareController"));

                SoftwareList.Clear();
                if (software != null)
                {
                    foreach (var item in software)
                    {
                        SoftwareList.Add(item);
                    }
                }

                FilterSoftware();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ПО: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterSoftware()
        {
            FilteredSoftwareList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in SoftwareList)
                {
                    FilteredSoftwareList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = SoftwareList.Where(s =>
                    (s.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (s.Version?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredSoftwareList.Add(item);
                }
            }
        }

        public async Task<bool> AddSoftwareAsync(Software software)
        {
            try
            {
                var success = await _apiService.AddItemAsync("SoftwareController", software);
                if (success)
                {
                    _cacheService.Remove("software_page_list");
                    await LoadSoftwareAsync();
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

        public async Task<bool> UpdateSoftwareAsync(int id, Software software)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("SoftwareController", id, software);
                if (success)
                {
                    _cacheService.Remove("software_page_list");
                    await LoadSoftwareAsync();
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

        public async Task<bool> DeleteSoftwareAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это ПО?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("SoftwareController", id);
                if (success)
                {
                    _cacheService.Remove("software_page_list");
                    await LoadSoftwareAsync();
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