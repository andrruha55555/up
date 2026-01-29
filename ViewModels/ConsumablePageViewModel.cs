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
    public class ConsumablePageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Consumable> _consumableList;
        private Consumable _selectedConsumable;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Consumable> ConsumableList
        {
            get => _consumableList;
            set
            {
                _consumableList = value;
                OnPropertyChanged();
            }
        }

        public Consumable SelectedConsumable
        {
            get => _selectedConsumable;
            set
            {
                _selectedConsumable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConsumableSelected));
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
                FilterConsumables();
            }
        }

        public bool IsConsumableSelected => SelectedConsumable != null;

        public ObservableCollection<Consumable> FilteredConsumableList { get; set; }

        public ConsumablePageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            ConsumableList = new ObservableCollection<Consumable>();
            FilteredConsumableList = new ObservableCollection<Consumable>();
        }

        public async Task LoadConsumablesAsync()
        {
            IsLoading = true;
            try
            {
                var consumables = await _cacheService.GetOrSetAsync("consumables_page_list",
                    async () => await _apiService.GetListAsync<Consumable>("ConsumablesController"));

                ConsumableList.Clear();
                if (consumables != null)
                {
                    foreach (var item in consumables)
                    {
                        ConsumableList.Add(item);
                    }
                }

                FilterConsumables();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расходников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterConsumables()
        {
            FilteredConsumableList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in ConsumableList)
                {
                    FilteredConsumableList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = ConsumableList.Where(c =>
                    (c.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (c.Description?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredConsumableList.Add(item);
                }
            }
        }

        public async Task<bool> AddConsumableAsync(Consumable consumable)
        {
            try
            {
                var success = await _apiService.AddItemAsync("ConsumablesController", consumable);
                if (success)
                {
                    _cacheService.Remove("consumables_page_list");
                    await LoadConsumablesAsync();
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

        public async Task<bool> UpdateConsumableAsync(int id, Consumable consumable)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("ConsumablesController", id, consumable);
                if (success)
                {
                    _cacheService.Remove("consumables_page_list");
                    await LoadConsumablesAsync();
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

        public async Task<bool> DeleteConsumableAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот расходник?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("ConsumablesController", id);
                if (success)
                {
                    _cacheService.Remove("consumables_page_list");
                    await LoadConsumablesAsync();
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