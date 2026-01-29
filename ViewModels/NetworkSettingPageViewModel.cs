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
    public class NetworkSettingPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<NetworkSetting> _networkSettingList;
        private NetworkSetting _selectedNetworkSetting;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<NetworkSetting> NetworkSettingList
        {
            get => _networkSettingList;
            set
            {
                _networkSettingList = value;
                OnPropertyChanged();
            }
        }

        public NetworkSetting SelectedNetworkSetting
        {
            get => _selectedNetworkSetting;
            set
            {
                _selectedNetworkSetting = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNetworkSettingSelected));
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
                FilterNetworkSettings();
            }
        }

        public bool IsNetworkSettingSelected => SelectedNetworkSetting != null;

        public ObservableCollection<NetworkSetting> FilteredNetworkSettingList { get; set; }

        public NetworkSettingPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            NetworkSettingList = new ObservableCollection<NetworkSetting>();
            FilteredNetworkSettingList = new ObservableCollection<NetworkSetting>();
        }

        public async Task LoadNetworkSettingsAsync()
        {
            IsLoading = true;
            try
            {
                var settings = await _cacheService.GetOrSetAsync("network_settings_page_list",
                    async () => await _apiService.GetListAsync<NetworkSetting>("NetworkSettingsController"));

                NetworkSettingList.Clear();
                if (settings != null)
                {
                    foreach (var item in settings)
                    {
                        NetworkSettingList.Add(item);
                    }
                }

                FilterNetworkSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сетевых настроек: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterNetworkSettings()
        {
            FilteredNetworkSettingList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in NetworkSettingList)
                {
                    FilteredNetworkSettingList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = NetworkSettingList.Where(n =>
                    (n.IpAddress?.ToLower().Contains(searchLower) ?? false) ||
                    (n.Gateway?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredNetworkSettingList.Add(item);
                }
            }
        }

        public async Task<bool> AddNetworkSettingAsync(NetworkSetting networkSetting)
        {
            try
            {
                var success = await _apiService.AddItemAsync("NetworkSettingsController", networkSetting);
                if (success)
                {
                    _cacheService.Remove("network_settings_page_list");
                    await LoadNetworkSettingsAsync();
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

        public async Task<bool> UpdateNetworkSettingAsync(int id, NetworkSetting networkSetting)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("NetworkSettingsController", id, networkSetting);
                if (success)
                {
                    _cacheService.Remove("network_settings_page_list");
                    await LoadNetworkSettingsAsync();
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

        public async Task<bool> DeleteNetworkSettingAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эти сетевые настройки?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("NetworkSettingsController", id);
                if (success)
                {
                    _cacheService.Remove("network_settings_page_list");
                    await LoadNetworkSettingsAsync();
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