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
    public class InventoryPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly CacheService _cache;

        public ObservableCollection<Inventory> InventoryList { get; } = new();
        public ObservableCollection<Inventory> FilteredInventoryList { get; } = new();

        private Inventory? _selectedInventory;
        public Inventory? SelectedInventory
        {
            get => _selectedInventory;
            set { _selectedInventory = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterInventories(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public InventoryPageViewModel(ApiService api, CacheService cache)
        {
            _api = api;
            _cache = cache;
        }

        public async Task LoadInventoriesAsync()
        {
            try
            {
                IsLoading = true;

                var list = await _cache.GetOrAddAsync("inventories", async () =>
                    await _api.GetListAsync<Inventory>("InventoriesController"));

                InventoryList.Clear();
                foreach (var i in list) InventoryList.Add(i);

                FilterInventories();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterInventories()
        {
            FilteredInventoryList.Clear();
            var q = (SearchText ?? "").Trim().ToLowerInvariant();

            var items = InventoryList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(x => (x.Name ?? "").ToLowerInvariant().Contains(q));

            foreach (var i in items) FilteredInventoryList.Add(i);
        }

        public async Task<bool> AddInventoryAsync(Inventory item)
        {
            var ok = await _api.AddItemAsync("InventoriesController", item);
            _cache.Remove("inventories");
            await LoadInventoriesAsync();
            return ok;
        }

        public async Task<bool> UpdateInventoryAsync(int id, Inventory item)
        {
            var ok = await _api.UpdateItemAsync("InventoriesController", id, item);
            _cache.Remove("inventories");
            await LoadInventoriesAsync();
            return ok;
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var ok = await _api.DeleteItemAsync("InventoriesController", id);
            _cache.Remove("inventories");
            await LoadInventoriesAsync();
            return ok;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
