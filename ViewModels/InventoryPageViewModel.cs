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
    public class InventoryPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Inventory> _inventoryList;
        private Inventory _selectedInventory;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Inventory> InventoryList
        {
            get => _inventoryList;
            set
            {
                _inventoryList = value;
                OnPropertyChanged();
            }
        }

        public Inventory SelectedInventory
        {
            get => _selectedInventory;
            set
            {
                _selectedInventory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInventorySelected));
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
                FilterInventories();
            }
        }

        public bool IsInventorySelected => SelectedInventory != null;

        public ObservableCollection<Inventory> FilteredInventoryList { get; set; }

        public InventoryPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            InventoryList = new ObservableCollection<Inventory>();
            FilteredInventoryList = new ObservableCollection<Inventory>();
        }

        public async Task LoadInventoriesAsync()
        {
            IsLoading = true;
            try
            {
                var inventories = await _cacheService.GetOrSetAsync("inventories_page_list",
                    async () => await _apiService.GetListAsync<Inventory>("InventoriesController"));

                InventoryList.Clear();
                if (inventories != null)
                {
                    foreach (var item in inventories)
                    {
                        InventoryList.Add(item);
                    }
                }

                FilterInventories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки инвентаризаций: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterInventories()
        {
            FilteredInventoryList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in InventoryList)
                {
                    FilteredInventoryList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = InventoryList.Where(i =>
                    (i.Name?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredInventoryList.Add(item);
                }
            }
        }

        public async Task<bool> AddInventoryAsync(Inventory inventory)
        {
            try
            {
                var success = await _apiService.AddItemAsync("InventoriesController", inventory);
                if (success)
                {
                    _cacheService.Remove("inventories_page_list");
                    await LoadInventoriesAsync();
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

        public async Task<bool> UpdateInventoryAsync(int id, Inventory inventory)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("InventoriesController", id, inventory);
                if (success)
                {
                    _cacheService.Remove("inventories_page_list");
                    await LoadInventoriesAsync();
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

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту инвентаризацию?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("InventoriesController", id);
                if (success)
                {
                    _cacheService.Remove("inventories_page_list");
                    await LoadInventoriesAsync();
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