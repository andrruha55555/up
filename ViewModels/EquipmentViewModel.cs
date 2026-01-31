using AdminUP.Models;
using AdminUP.Services;
using AdminUP.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.ViewModels
{
    public class EquipmentPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Equipment> _equipmentList;
        private Equipment _selectedEquipment;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Equipment> EquipmentList
        {
            get => _equipmentList;
            set
            {
                _equipmentList = value;
                OnPropertyChanged();
            }
        }

        public Equipment SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEquipmentSelected));
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
                FilterEquipment();
            }
        }

        public bool IsEquipmentSelected => SelectedEquipment != null;

        public ObservableCollection<Equipment> FilteredEquipmentList { get; set; }

        public EquipmentPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            EquipmentList = new ObservableCollection<Equipment>();
            FilteredEquipmentList = new ObservableCollection<Equipment>();
        }

        public async Task LoadEquipmentAsync()
        {
            IsLoading = true;
            try
            {
                var equipment = await _cacheService.GetOrSetAsync("equipment_page_list",
                    async () => await _apiService.GetListAsync<Equipment>("EquipmentController"));

                EquipmentList.Clear();
                if (equipment != null)
                {
                    foreach (var item in equipment)
                    {
                        EquipmentList.Add(item);
                    }
                }

                FilterEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оборудования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterEquipment()
        {
            FilteredEquipmentList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in EquipmentList)
                {
                    FilteredEquipmentList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = EquipmentList.Where(e =>
                    (e.name?.ToLower().Contains(searchLower) ?? false) ||
                    (e.inventory_number?.ToLower().Contains(searchLower) ?? false) ||
                    (e.comment?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredEquipmentList.Add(item);
                }
            }
        }

        public async Task<bool> AddEquipmentAsync(Equipment equipment)
        {
            try
            {
                var success = await _apiService.AddItemAsync("EquipmentController", equipment);
                if (success)
                {
                    _cacheService.Remove("equipment_page_list");
                    await LoadEquipmentAsync();
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

        public async Task<bool> UpdateEquipmentAsync(int id, Equipment equipment)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("EquipmentController", id, equipment);
                if (success)
                {
                    _cacheService.Remove("equipment_page_list");
                    await LoadEquipmentAsync();
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

        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это оборудование?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("EquipmentController", id);
                if (success)
                {
                    _cacheService.Remove("equipment_page_list");
                    await LoadEquipmentAsync();
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