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
    public class InventoryItemPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<InventoryItem> _inventoryItemList;
        private InventoryItem _selectedInventoryItem;
        private bool _isLoading;
        private string _searchText;
        private int? _selectedInventoryId;

        public ObservableCollection<InventoryItem> InventoryItemList
        {
            get => _inventoryItemList;
            set
            {
                _inventoryItemList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Inventory> InventoryList { get; set; }

        public InventoryItem SelectedInventoryItem
        {
            get => _selectedInventoryItem;
            set
            {
                _selectedInventoryItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInventoryItemSelected));
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
                FilterInventoryItems();
            }
        }

        public int? SelectedInventoryId
        {
            get => _selectedInventoryId;
            set
            {
                _selectedInventoryId = value;
                OnPropertyChanged();
                FilterInventoryItems();
            }
        }

        public bool IsInventoryItemSelected => SelectedInventoryItem != null;

        public ObservableCollection<InventoryItem> FilteredInventoryItemList { get; set; }

        public InventoryItemPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            InventoryItemList = new ObservableCollection<InventoryItem>();
            InventoryList = new ObservableCollection<Inventory>();
            FilteredInventoryItemList = new ObservableCollection<InventoryItem>();
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadInventoryItemsAsync(),
                    LoadInventoriesAsync()
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadInventoryItemsAsync()
        {
            var items = await _cacheService.GetOrSetAsync("inventory_items_page_list",
                async () => await _apiService.GetListAsync<InventoryItem>("InventoryItemsController"));

            InventoryItemList.Clear();
            if (items != null)
            {
                foreach (var item in items)
                {
                    InventoryItemList.Add(item);
                }
            }

            FilterInventoryItems();
        }

        private async Task LoadInventoriesAsync()
        {
            var inventories = await _cacheService.GetOrSetAsync("inventories_for_items",
                async () => await _apiService.GetListAsync<Inventory>("InventoriesController"));

            InventoryList.Clear();
            if (inventories != null)
            {
                foreach (var item in inventories)
                {
                    InventoryList.Add(item);
                }
            }
        }

        public void FilterInventoryItems()
        {
            FilteredInventoryItemList.Clear();

            IEnumerable<InventoryItem> filtered = InventoryItemList;

            if (SelectedInventoryId.HasValue && SelectedInventoryId > 0)
            {
                filtered = filtered.Where(i => i.InventoryId == SelectedInventoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(i =>
                    (i.Comment?.ToLower().Contains(searchLower) ?? false));
            }

            foreach (var item in filtered)
            {
                FilteredInventoryItemList.Add(item);
            }
        }

        public async Task<bool> AddInventoryItemAsync(InventoryItem inventoryItem)
        {
            try
            {
                var success = await _apiService.AddItemAsync("InventoryItemsController", inventoryItem);
                if (success)
                {
                    _cacheService.Remove("inventory_items_page_list");
                    await LoadInventoryItemsAsync();
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

        public async Task<bool> UpdateInventoryItemAsync(int id, InventoryItem inventoryItem)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("InventoryItemsController", id, inventoryItem);
                if (success)
                {
                    _cacheService.Remove("inventory_items_page_list");
                    await LoadInventoryItemsAsync();
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

        public async Task<bool> DeleteInventoryItemAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот элемент инвентаризации?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("InventoryItemsController", id);
                if (success)
                {
                    _cacheService.Remove("inventory_items_page_list");
                    await LoadInventoryItemsAsync();
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