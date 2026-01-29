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
    public class ConsumableCharacteristicPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<ConsumableCharacteristic> _characteristicList;
        private ConsumableCharacteristic _selectedCharacteristic;
        private bool _isLoading;
        private string _searchText;
        private int? _selectedConsumableId;

        public ObservableCollection<ConsumableCharacteristic> CharacteristicList
        {
            get => _characteristicList;
            set
            {
                _characteristicList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Consumable> ConsumableList { get; set; }

        public ConsumableCharacteristic SelectedCharacteristic
        {
            get => _selectedCharacteristic;
            set
            {
                _selectedCharacteristic = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCharacteristicSelected));
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
                FilterCharacteristics();
            }
        }

        public int? SelectedConsumableId
        {
            get => _selectedConsumableId;
            set
            {
                _selectedConsumableId = value;
                OnPropertyChanged();
                FilterCharacteristics();
            }
        }

        public bool IsCharacteristicSelected => SelectedCharacteristic != null;

        public ObservableCollection<ConsumableCharacteristic> FilteredCharacteristicList { get; set; }

        public ConsumableCharacteristicPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            CharacteristicList = new ObservableCollection<ConsumableCharacteristic>();
            ConsumableList = new ObservableCollection<Consumable>();
            FilteredCharacteristicList = new ObservableCollection<ConsumableCharacteristic>();
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadCharacteristicsAsync(),
                    LoadConsumablesAsync()
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCharacteristicsAsync()
        {
            var characteristics = await _cacheService.GetOrSetAsync("consumable_characteristics_page_list",
                async () => await _apiService.GetListAsync<ConsumableCharacteristic>("ConsumableCharacteristicsController"));

            CharacteristicList.Clear();
            if (characteristics != null)
            {
                foreach (var item in characteristics)
                {
                    CharacteristicList.Add(item);
                }
            }

            FilterCharacteristics();
        }

        private async Task LoadConsumablesAsync()
        {
            var consumables = await _cacheService.GetOrSetAsync("consumables_for_characteristics",
                async () => await _apiService.GetListAsync<Consumable>("ConsumablesController"));

            ConsumableList.Clear();
            if (consumables != null)
            {
                foreach (var item in consumables)
                {
                    ConsumableList.Add(item);
                }
            }
        }

        public void FilterCharacteristics()
        {
            FilteredCharacteristicList.Clear();

            IEnumerable<ConsumableCharacteristic> filtered = CharacteristicList;

            if (SelectedConsumableId.HasValue && SelectedConsumableId > 0)
            {
                filtered = filtered.Where(c => c.ConsumableId == SelectedConsumableId.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(c =>
                    (c.CharacteristicName?.ToLower().Contains(searchLower) ?? false) ||
                    (c.CharacteristicValue?.ToLower().Contains(searchLower) ?? false));
            }

            foreach (var item in filtered)
            {
                FilteredCharacteristicList.Add(item);
            }
        }

        public async Task<bool> AddCharacteristicAsync(ConsumableCharacteristic characteristic)
        {
            try
            {
                var success = await _apiService.AddItemAsync("ConsumableCharacteristicsController", characteristic);
                if (success)
                {
                    _cacheService.Remove("consumable_characteristics_page_list");
                    await LoadCharacteristicsAsync();
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

        public async Task<bool> UpdateCharacteristicAsync(int id, ConsumableCharacteristic characteristic)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("ConsumableCharacteristicsController", id, characteristic);
                if (success)
                {
                    _cacheService.Remove("consumable_characteristics_page_list");
                    await LoadCharacteristicsAsync();
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

        public async Task<bool> DeleteCharacteristicAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту характеристику?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("ConsumableCharacteristicsController", id);
                if (success)
                {
                    _cacheService.Remove("consumable_characteristics_page_list");
                    await LoadCharacteristicsAsync();
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