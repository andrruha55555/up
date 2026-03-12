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
    /// <summary>
    /// ViewModel страницы элементов инвентаризации.
    /// п. 1.8 ТЗ: пользователи (не admin) видят только то оборудование,
    /// которое закреплено за ними (responsible_user_id == CurrentUserId).
    /// </summary>
    public class InventoryItemPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        /// <summary>Все элементы инвентаризации (полный список из API)</summary>
        public ObservableCollection<InventoryItem> InventoryItemList { get; } = new();

        /// <summary>Отфильтрованный список для отображения в DataGrid</summary>
        public ObservableCollection<InventoryItem> FilteredInventoryItemList { get; } = new();

        /// <summary>Список инвентаризаций для фильтра-комбобокса</summary>
        public ObservableCollection<Inventory> InventoryList { get; } = new();

        // ─── Свойства ─────────────────────────────────────────────────────────

        private InventoryItem? _selectedInventoryItem;
        /// <summary>Выбранный элемент в DataGrid</summary>
        public InventoryItem? SelectedInventoryItem
        {
            get => _selectedInventoryItem;
            set { _selectedInventoryItem = value; OnPropertyChanged(); }
        }

        private int? _selectedInventoryId;
        /// <summary>Фильтр по инвентаризации</summary>
        public int? SelectedInventoryId
        {
            get => _selectedInventoryId;
            set { _selectedInventoryId = value; OnPropertyChanged(); FilterInventoryItems(); }
        }

        private string _searchText = "";
        /// <summary>Текст поиска</summary>
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); }
        }

        // ─── Хранит ID оборудования, за которым закреплён текущий user ──────

        /// <summary>
        /// Список equipment_id, закреплённых за текущим пользователем.
        /// Используется для фильтрации если пользователь не admin.
        /// </summary>
        private HashSet<int> _myEquipmentIds = new();

        public InventoryItemPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
        }

        // ─── Загрузка ─────────────────────────────────────────────────────────

        public async Task LoadDataAsync()
        {
            // Загружаем список инвентаризаций
            var inventories = await _cacheService.GetOrAddAsync("inventories_page_list", async () =>
                await _apiService.GetListAsync<Inventory>("InventoriesController"));

            InventoryList.Clear();
            if (inventories != null)
                foreach (var x in inventories) InventoryList.Add(x);

            // Загружаем элементы инвентаризации
            var items = await _cacheService.GetOrAddAsync("inventory_items_page_list", async () =>
                await _apiService.GetListAsync<InventoryItem>("InventoryItemsController"));

            InventoryItemList.Clear();
            if (items != null)
                foreach (var x in items) InventoryItemList.Add(x);

            // ✅ п. 1.8 ТЗ: для не-admin строим список "своего" оборудования
            if (!App.AuthService.IsAdmin)
            {
                var equipment = await _cacheService.GetOrAddAsync("equipment_page_list", async () =>
                    await _apiService.GetListAsync<Equipment>("EquipmentController"));

                _myEquipmentIds = equipment?
                    .Where(e => e.responsible_user_id == App.AuthService.CurrentUserId)
                    .Select(e => e.id)
                    .ToHashSet() ?? new HashSet<int>();
            }

            FilterInventoryItems();
        }

        // ─── Фильтрация ───────────────────────────────────────────────────────

        /// <summary>
        /// Фильтрует список с учётом:
        /// — выбранной инвентаризации;
        /// — текста поиска;
        /// — роли пользователя (не-admin видят только своё оборудование).
        /// </summary>
        public void FilterInventoryItems()
        {
            var q = (SearchText ?? "").Trim().ToLowerInvariant();
            IEnumerable<InventoryItem> items = InventoryItemList;

            // Фильтр по инвентаризации
            if (SelectedInventoryId.HasValue)
                items = items.Where(x => x.inventory_id == SelectedInventoryId.Value);

            // ✅ п. 1.8 ТЗ: не-admin видят только своё оборудование
            if (!App.AuthService.IsAdmin && _myEquipmentIds.Count > 0)
                items = items.Where(x => _myEquipmentIds.Contains(x.equipment_id));
            else if (!App.AuthService.IsAdmin && _myEquipmentIds.Count == 0)
            {
                // Если у пользователя нет закреплённого оборудования — ничего не показываем
                FilteredInventoryItemList.Clear();
                return;
            }

            // Текстовый поиск
            if (!string.IsNullOrWhiteSpace(q))
            {
                items = items.Where(x =>
                    x.id.ToString().Contains(q) ||
                    x.inventory_id.ToString().Contains(q) ||
                    x.equipment_id.ToString().Contains(q) ||
                    (x.comment ?? "").ToLowerInvariant().Contains(q));
            }

            FilteredInventoryItemList.Clear();
            foreach (var x in items.OrderBy(x => x.inventory_id).ThenBy(x => x.equipment_id))
                FilteredInventoryItemList.Add(x);
        }

        // ─── CRUD ─────────────────────────────────────────────────────────────

        public async Task AddInventoryItemAsync(InventoryItem item)
        {
            // Автоматически ставим проверяющего — текущего пользователя (п. 1.8 ТЗ)
            if (item.checked_by_user_id == 0 || item.checked_by_user_id == null)
                item.checked_by_user_id = App.AuthService.CurrentUserId;

            if (item.checked_at == null || item.checked_at == DateTime.MinValue)
                item.checked_at = DateTime.UtcNow;

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

        // ─── INotifyPropertyChanged ───────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
