using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdminUP.ViewModels
{
    public class InventoryItemPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        public ObservableCollection<InventoryItem> InventoryItemList { get; } = new();
        public ObservableCollection<InventoryItem> FilteredInventoryItemList { get; } = new();

        public ObservableCollection<Inventory> InventoryList { get; } = new();

        private InventoryItem? _selectedInventoryItem;
        public InventoryItem? SelectedInventoryItem
        {
            get => _selectedInventoryItem;
            set { _selectedInventoryItem = value; OnPropertyChanged(); }
        }

        private int? _selectedInventoryId;
        public int? SelectedInventoryId
        {
            get => _selectedInventoryId;
            set { _selectedInventoryId = value; OnPropertyChanged(); FilterInventoryItems(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); }
        }

        public InventoryItemPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
        }

        public async Task LoadDataAsync()
        {
            // inventories
            var inventories = await _cacheService.GetOrAddAsync("inventories_page_list", async () =>
                await _apiService.GetListAsync<Inventory>("InventoriesController"));

            InventoryList.Clear();
            if (inventories != null)
                foreach (var x in inventories) InventoryList.Add(x);

            // items
            var items = await _cacheService.GetOrAddAsync("inventory_items_page_list", async () =>
                await _apiService.GetListAsync<InventoryItem>("InventoryItemsController"));

            InventoryItemList.Clear();
            if (items != null)
                foreach (var x in items) InventoryItemList.Add(x);

            FilterInventoryItems();
        }

        public void FilterInventoryItems()
        {
            var q = (SearchText ?? "").Trim().ToLowerInvariant();
            IEnumerable<InventoryItem> items = InventoryItemList;

            if (SelectedInventoryId.HasValue)
                items = items.Where(x => x.inventory_id == SelectedInventoryId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                items = items.Where(x =>
                    x.id.ToString().Contains(q) ||
                    x.inventory_id.ToString().Contains(q) ||
                    x.equipment_id.ToString().Contains(q) ||
                    (x.comment ?? "").ToLowerInvariant().Contains(q));
            }

            FilteredInventoryItemList.Clear();
            foreach (var x in items) FilteredInventoryItemList.Add(x);
        }

        public async Task AddInventoryItemAsync(InventoryItem item)
        {
            await _apiService.AddItemAsync("InventoryItemsController", item);
            _cacheService.Remove("inventory_items_page_list");
            await LoadDataAsync();
        }

        public async Task UpdateInventoryItemAsync(int id, InventoryItem item)
        {
            await _apiService.UpdateItemAsync("InventoryItemsController", id, item);
            _cacheService.Remove("inventory_items_page_list");
            await LoadDataAsync();
        }

        public async Task DeleteInventoryItemAsync(int id)
        {
            await _apiService.DeleteItemAsync("InventoryItemsController", id);
            _cacheService.Remove("inventory_items_page_list");
            await LoadDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}