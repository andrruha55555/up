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
    public class EquipmentTypePageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<EquipmentType> _equipmentTypeList;
        private EquipmentType _selectedEquipmentType;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<EquipmentType> EquipmentTypeList
        {
            get => _equipmentTypeList;
            set
            {
                _equipmentTypeList = value;
                OnPropertyChanged();
            }
        }

        public EquipmentType SelectedEquipmentType
        {
            get => _selectedEquipmentType;
            set
            {
                _selectedEquipmentType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEquipmentTypeSelected));
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
                FilterEquipmentTypes();
            }
        }

        public bool IsEquipmentTypeSelected => SelectedEquipmentType != null;

        public ObservableCollection<EquipmentType> FilteredEquipmentTypeList { get; set; }

        public EquipmentTypePageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            EquipmentTypeList = new ObservableCollection<EquipmentType>();
            FilteredEquipmentTypeList = new ObservableCollection<EquipmentType>();
        }

        public async Task LoadEquipmentTypesAsync()
        {
            IsLoading = true;
            try
            {
                var types = await _cacheService.GetOrSetAsync("equipment_types_page_list",
                    async () => await _apiService.GetListAsync<EquipmentType>("EquipmentTypesController"));

                EquipmentTypeList.Clear();
                if (types != null)
                {
                    foreach (var item in types)
                    {
                        EquipmentTypeList.Add(item);
                    }
                }

                FilterEquipmentTypes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов оборудования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterEquipmentTypes()
        {
            FilteredEquipmentTypeList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in EquipmentTypeList)
                {
                    FilteredEquipmentTypeList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = EquipmentTypeList.Where(t =>
                    (t.Name?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredEquipmentTypeList.Add(item);
                }
            }
        }

        public async Task<bool> AddEquipmentTypeAsync(EquipmentType equipmentType)
        {
            try
            {
                var success = await _apiService.AddItemAsync("EquipmentTypesController", equipmentType);
                if (success)
                {
                    _cacheService.Remove("equipment_types_page_list");
                    await LoadEquipmentTypesAsync();
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

        public async Task<bool> UpdateEquipmentTypeAsync(int id, EquipmentType equipmentType)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("EquipmentTypesController", id, equipmentType);
                if (success)
                {
                    _cacheService.Remove("equipment_types_page_list");
                    await LoadEquipmentTypesAsync();
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

        public async Task<bool> DeleteEquipmentTypeAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот тип оборудования?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("EquipmentTypesController", id);
                if (success)
                {
                    _cacheService.Remove("equipment_types_page_list");
                    await LoadEquipmentTypesAsync();
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