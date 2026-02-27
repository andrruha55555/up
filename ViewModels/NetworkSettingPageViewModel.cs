using AdminUP.Models;
using AdminUP.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdminUP.ViewModels
{
    public class NetworkSettingPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        public ObservableCollection<NetworkSetting> NetworkSettings { get; } = new();
        public ObservableCollection<NetworkSetting> FilteredNetworkSettings { get; } = new();

        private NetworkSetting? _selectedNetworkSetting;
        public NetworkSetting? SelectedNetworkSetting
        {
            get => _selectedNetworkSetting;
            set { _selectedNetworkSetting = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); }
        }

        public NetworkSettingPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
        }

        // ТВОЙ code-behind ждёт LoadDataAsync()
        public async Task LoadDataAsync()
        {
            var list = await _cacheService.GetOrAddAsync("network_settings", async () =>
                await _apiService.GetListAsync<NetworkSetting>("NetworkSettingsController"));

            NetworkSettings.Clear();
            if (list != null)
                foreach (var x in list) NetworkSettings.Add(x);

            FilterNetwork();
        }

        // ТВОЙ code-behind вызывает FilterNetwork() без аргументов
        public void FilterNetwork()
        {
            FilterNetwork(SearchText);
        }

        public void FilterNetwork(string search)
        {
            var q = (search ?? "").Trim().ToLowerInvariant();

            IEnumerable<NetworkSetting> items = NetworkSettings;

            if (!string.IsNullOrWhiteSpace(q))
            {
                items = items.Where(x =>
                    (x.ip_address ?? "").ToLowerInvariant().Contains(q) ||
                    (x.gateway ?? "").ToLowerInvariant().Contains(q) ||
                    (x.dns1 ?? "").ToLowerInvariant().Contains(q) ||
                    (x.dns2 ?? "").ToLowerInvariant().Contains(q));
            }

            FilteredNetworkSettings.Clear();
            foreach (var x in items) FilteredNetworkSettings.Add(x);
        }

        public async Task AddNetworkSettingAsync(NetworkSetting item)
        {
            await _apiService.AddItemAsync("NetworkSettingsController", item);
            _cacheService.Remove("network_settings");
            await LoadDataAsync();
        }

        public async Task UpdateNetworkSettingAsync(int id, NetworkSetting item)
        {
            await _apiService.UpdateItemAsync("NetworkSettingsController", id, item);
            _cacheService.Remove("network_settings");
            await LoadDataAsync();
        }

        public async Task DeleteNetworkSettingAsync(int id)
        {
            await _apiService.DeleteItemAsync("NetworkSettingsController", id);
            _cacheService.Remove("network_settings");
            await LoadDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}