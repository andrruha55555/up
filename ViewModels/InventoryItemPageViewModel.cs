
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
    /// <summary>Строка таблицы позиций инвентаризации с именами вместо ID</summary>
    public class InventoryItemRow
    {
        public InventoryItem Item { get; }
        public int id => Item.id;
        public int inventory_id => Item.inventory_id;
        public int equipment_id => Item.equipment_id;
        public string? Comment => Item.comment;

        public string EquipmentName { get; }
        public string InventoryNumber { get; }
        public string CheckedByUserName { get; }
        public string CheckedAtStr => Item.checked_at?.ToString("dd.MM.yyyy HH:mm") ?? "—";
        public bool IsChecked => Item.checked_by_user_id.HasValue && Item.checked_by_user_id > 0;

        public InventoryItemRow(InventoryItem item,
            Dictionary<int, Equipment> equipmentMap,
            Dictionary<int, User> usersMap)
        {
            Item = item;
            if (equipmentMap.TryGetValue(item.equipment_id, out var eq))
            {
                EquipmentName = eq.name ?? $"ID {item.equipment_id}";
                InventoryNumber = eq.inventory_number ?? "";
            }
            else
            {
                EquipmentName = $"ID {item.equipment_id}";
                InventoryNumber = "";
            }
            if (item.checked_by_user_id.HasValue &&
                usersMap.TryGetValue(item.checked_by_user_id.Value, out var u))
                CheckedByUserName = u.FullName;
            else
                CheckedByUserName = "—";
        }
    }

    public class InventoryItemPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private List<InventoryItemRow> _allRows = new();
        public ObservableCollection<InventoryItemRow> FilteredInventoryItemList { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItemList { get; } = new();
        public ObservableCollection<Inventory> InventoryList { get; } = new();

        private InventoryItemRow? _selectedRow;
        public InventoryItemRow? SelectedInventoryItem
        {
            get => _selectedRow;
            set { _selectedRow = value; OnPropertyChanged(); }
        }

        private int? _selectedInventoryId;
        public int? SelectedInventoryId
        {
            get => _selectedInventoryId;
            set { _selectedInventoryId = value; OnPropertyChanged(); FilterInventoryItems(); UpdateCounts(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); }
        }

        // Счётчики для выбранной инвентаризации
        public bool HasSelectedInventory => _selectedInventoryId.HasValue;
        public int TotalCount { get; private set; }
        public int CheckedCount { get; private set; }
        public int UncheckedCount { get; private set; }

        public InventoryItemPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
        }

        public async Task LoadDataAsync()
        {
            var inventories = await _cacheService.GetOrAddAsync("inventories_page_list",
                async () => await _apiService.GetListAsync<Inventory>("InventoriesController"));
            InventoryList.Clear();
            if (inventories != null) foreach (var x in inventories) InventoryList.Add(x);

            var items = await _cacheService.GetOrAddAsync("inventory_items_page_list",
                async () => await _apiService.GetListAsync<InventoryItem>("InventoryItemsController"));

            var equipment = await _apiService.GetListAsync<Equipment>("EquipmentController");
            var users = await _apiService.GetListAsync<User>("UsersController");

            var eqMap = (equipment ?? new()).ToDictionary(e => e.id, e => e);
            var uMap = (users ?? new()).ToDictionary(u => u.id, u => u);

            InventoryItemList.Clear();
            _allRows.Clear();
            if (items != null)
            {
                foreach (var x in items)
                {
                    InventoryItemList.Add(x);
                    _allRows.Add(new InventoryItemRow(x, eqMap, uMap));
                }
            }
            FilterInventoryItems();
            UpdateCounts();
        }

        public void FilterInventoryItems()
        {
            var q = (_searchText ?? "").Trim().ToLowerInvariant();
            IEnumerable<InventoryItemRow> src = _allRows;

            if (_selectedInventoryId.HasValue)
                src = src.Where(r => r.inventory_id == _selectedInventoryId.Value);

            if (!string.IsNullOrWhiteSpace(q))
                src = src.Where(r =>
                    r.EquipmentName.ToLowerInvariant().Contains(q) ||
                    r.InventoryNumber.ToLowerInvariant().Contains(q) ||
                    (r.Comment ?? "").ToLowerInvariant().Contains(q) ||
                    r.CheckedByUserName.ToLowerInvariant().Contains(q));

            FilteredInventoryItemList.Clear();
            foreach (var r in src) FilteredInventoryItemList.Add(r);
        }

        private void UpdateCounts()
        {
            IEnumerable<InventoryItemRow> src = _allRows;
            if (_selectedInventoryId.HasValue)
                src = src.Where(r => r.inventory_id == _selectedInventoryId.Value);

            var list = src.ToList();
            TotalCount = list.Count;
            CheckedCount = list.Count(r => r.IsChecked);
            UncheckedCount = list.Count(r => !r.IsChecked);
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(CheckedCount));
            OnPropertyChanged(nameof(UncheckedCount));
            OnPropertyChanged(nameof(HasSelectedInventory));
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
