using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdminUP.ViewModels
{
    public class NetworkSettingPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly CacheService _cache;

        public ObservableCollection<NetworkSetting> NetworkList { get; } = new();
        public ObservableCollection<NetworkSetting> FilteredNetworkList { get; } = new();
        public ObservableCollection<Equipment> EquipmentList { get; } = new();

        private NetworkSetting? _selected;
        public NetworkSetting? SelectedNetworkSetting
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterNetwork(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public NetworkSettingPageViewModel(ApiService api, CacheService cache)
        {
            _api = api;
            _cache = cache;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var equipment = await _cache.GetOrAddAsync("equipment", async () =>
                    await _api.GetListAsync<Equipment>("EquipmentController"));

                EquipmentList.Clear();
                foreach (var e in equipment) EquipmentList.Add(e);

                var list = await _cache.GetOrAddAsync("network_settings", async () =>
                    await _api.GetListAsync<NetworkSetting>("NetworkSettingsController"));

                NetworkList.Clear();
                foreach (var n in list)
                {
                    
                }

                FilterNetwork();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterNetwork()
        {
            FilteredNetworkList.Clear();
            var q = (SearchText ?? "").Trim().ToLowerInvariant();

            var items = NetworkList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(x =>
                    (x.ip_address ?? "").ToLowerInvariant().Contains(q) ||
                    (x.gateway ?? "").ToLowerInvariant().Contains(q));

            foreach (var i in items) FilteredNetworkList.Add(i);
        }

        public async Task<bool> AddNetworkSettingAsync(NetworkSetting item)
        {
            var ok = await _api.AddItemAsync("NetworkSettingsController", item);
            _cache.Remove("network_settings");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> UpdateNetworkSettingAsync(int id, NetworkSetting item)
        {
            var ok = await _api.UpdateItemAsync("NetworkSettingsController", id, item);
            _cache.Remove("network_settings");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> DeleteNetworkSettingAsync(int id)
        {
            var ok = await _api.DeleteItemAsync("NetworkSettingsController", id);
            _cache.Remove("network_settings");
            await LoadDataAsync();
            return ok;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
