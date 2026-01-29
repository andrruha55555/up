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
    public class ConsumableTypePageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<ConsumableType> _consumableTypeList;
        private ConsumableType _selectedConsumableType;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<ConsumableType> ConsumableTypeList
        {
            get => _consumableTypeList;
            set
            {
                _consumableTypeList = value;
                OnPropertyChanged();
            }
        }

        public ConsumableType SelectedConsumableType
        {
            get => _selectedConsumableType;
            set
            {
                _selectedConsumableType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConsumableTypeSelected));
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
                FilterConsumableTypes();
            }
        }

        public bool IsConsumableTypeSelected => SelectedConsumableType != null;

        public ObservableCollection<ConsumableType> FilteredConsumableTypeList { get; set; }

        public ConsumableTypePageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            ConsumableTypeList = new ObservableCollection<ConsumableType>();
            FilteredConsumableTypeList = new ObservableCollection<ConsumableType>();
        }

        public async Task LoadConsumableTypesAsync()
        {
            IsLoading = true;
            try
            {
                var types = await _cacheService.GetOrSetAsync("consumable_types_page_list",
                    async () => await _apiService.GetListAsync<ConsumableType>("ConsumableTypesController"));

                ConsumableTypeList.Clear();
                if (types != null)
                {
                    foreach (var item in types)
                    {
                        ConsumableTypeList.Add(item);
                    }
                }

                FilterConsumableTypes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов расходников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterConsumableTypes()
        {
            FilteredConsumableTypeList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in ConsumableTypeList)
                {
                    FilteredConsumableTypeList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = ConsumableTypeList.Where(t =>
                    (t.Name?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredConsumableTypeList.Add(item);
                }
            }
        }

        public async Task<bool> AddConsumableTypeAsync(ConsumableType consumableType)
        {
            try
            {
                var success = await _apiService.AddItemAsync("ConsumableTypesController", consumableType);
                if (success)
                {
                    _cacheService.Remove("consumable_types_page_list");
                    await LoadConsumableTypesAsync();
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

        public async Task<bool> UpdateConsumableTypeAsync(int id, ConsumableType consumableType)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("ConsumableTypesController", id, consumableType);
                if (success)
                {
                    _cacheService.Remove("consumable_types_page_list");
                    await LoadConsumableTypesAsync();
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

        public async Task<bool> DeleteConsumableTypeAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот тип расходника?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("ConsumableTypesController", id);
                if (success)
                {
                    _cacheService.Remove("consumable_types_page_list");
                    await LoadConsumableTypesAsync();
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