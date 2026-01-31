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
    public class StatusPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Status> _statusList;
        private Status _selectedStatus;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Status> StatusList
        {
            get => _statusList;
            set
            {
                _statusList = value;
                OnPropertyChanged();
            }
        }

        public Status SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStatusSelected));
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
                FilterStatuses();
            }
        }

        public bool IsStatusSelected => SelectedStatus != null;

        public ObservableCollection<Status> FilteredStatusList { get; set; }

        public StatusPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            StatusList = new ObservableCollection<Status>();
            FilteredStatusList = new ObservableCollection<Status>();
        }

        public async Task LoadStatusesAsync()
        {
            IsLoading = true;
            try
            {
                var statuses = await _cacheService.GetOrSetAsync("statuses_page_list",
                    async () => await _apiService.GetListAsync<Status>("StatusesController"));

                StatusList.Clear();
                if (statuses != null)
                {
                    foreach (var item in statuses)
                    {
                        StatusList.Add(item);
                    }
                }

                FilterStatuses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterStatuses()
        {
            FilteredStatusList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in StatusList)
                {
                    FilteredStatusList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = StatusList.Where(s =>
                    (s.name?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredStatusList.Add(item);
                }
            }
        }

        public async Task<bool> AddStatusAsync(Status status)
        {
            try
            {
                var success = await _apiService.AddItemAsync("StatusesController", status);
                if (success)
                {
                    _cacheService.Remove("statuses_page_list");
                    await LoadStatusesAsync();
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

        public async Task<bool> UpdateStatusAsync(int id, Status status)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("StatusesController", id, status);
                if (success)
                {
                    _cacheService.Remove("statuses_page_list");
                    await LoadStatusesAsync();
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

        public async Task<bool> DeleteStatusAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот статус?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("StatusesController", id);
                if (success)
                {
                    _cacheService.Remove("statuses_page_list");
                    await LoadStatusesAsync();
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