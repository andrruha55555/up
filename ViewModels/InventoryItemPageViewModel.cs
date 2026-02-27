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
    public class InventoryItemPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();
        public ObservableCollection<InventoryItem> FilteredInventoryItems { get; } = new();

        // ТВОЙ code-behind ждёт InventoryList
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

        // ТВОЙ code-behind ждёт LoadDataAsync()
        public async Task LoadDataAsync()
        {
            var inventories = await _cacheService.GetOrAddAsync("inventories", async () =>
                await _apiService.GetListAsync<Inventory>("InventoriesController"));

            InventoryList.Clear();
            if (inventories != null)
                foreach (var x in inventories) InventoryList.Add(x);

            var list = await _cacheService.GetOrAddAsync("inventory_items", async () =>
                await _apiService.GetListAsync<InventoryItem>("InventoryItemsController"));

            InventoryItems.Clear();
            if (list != null)
                foreach (var x in list) InventoryItems.Add(x);

            FilterInventoryItems();
        }

        // ТВОЙ code-behind вызывает FilterInventoryItems() без аргументов
        public void FilterInventoryItems()
        {
            FilterInventoryItems(SearchText);
        }

        public void FilterInventoryItems(string search)
        {
            var q = (search ?? "").Trim().ToLowerInvariant();

            IEnumerable<InventoryItem> items = InventoryItems;

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

            FilteredInventoryItems.Clear();
            foreach (var x in items) FilteredInventoryItems.Add(x);
        }

        public async Task AddInventoryItemAsync(InventoryItem item)
        {
            await _apiService.AddItemAsync("InventoryItemsController", item);
            _cacheService.Remove("inventory_items");
            await LoadDataAsync();
        }

        public async Task UpdateInventoryItemAsync(int id, InventoryItem item)
        {
            await _apiService.UpdateItemAsync("InventoryItemsController", id, item);
            _cacheService.Remove("inventory_items");
            await LoadDataAsync();
        }

        public async Task DeleteInventoryItemAsync(int id)
        {
            await _apiService.DeleteItemAsync("InventoryItemsController", id);
            _cacheService.Remove("inventory_items");
            await LoadDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}