using AdminUP.Helpers;
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
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;
        private readonly AuthService _authService;
        private readonly ExportService _exportService;
        private Task LoadConsumablesAsync() => Task.CompletedTask;
        private Task LoadStatusesAsync() => Task.CompletedTask;
        private Task LoadEquipmentTypesAsync() => Task.CompletedTask;
        private Task LoadModelsAsync() => Task.CompletedTask;
        private Task LoadConsumableTypesAsync() => Task.CompletedTask;
        private Task LoadConsumableCharacteristicsAsync() => Task.CompletedTask;
        private Task LoadDevelopersAsync() => Task.CompletedTask;
        private Task LoadDirectionsAsync() => Task.CompletedTask;
        private Task LoadSoftwareAsync() => Task.CompletedTask;
        private Task LoadInventoriesAsync() => Task.CompletedTask;
        private Task LoadInventoryItemsAsync() => Task.CompletedTask;
        private Task LoadNetworkSettingsAsync() => Task.CompletedTask;

        // Коллекции
        public ObservableCollection<Equipment> EquipmentList { get; set; }
        public ObservableCollection<User> UserList { get; set; }
        public ObservableCollection<Classroom> ClassroomList { get; set; }
        public ObservableCollection<Consumable> ConsumableList { get; set; }
        public ObservableCollection<Status> StatusList { get; set; }
        public ObservableCollection<EquipmentType> EquipmentTypeList { get; set; }
        public ObservableCollection<ModelEntity> ModelList { get; set; }
        public ObservableCollection<ConsumableType> ConsumableTypeList { get; set; }
        public ObservableCollection<ConsumableCharacteristic> ConsumableCharacteristicList { get; set; }
        public ObservableCollection<Developer> DeveloperList { get; set; }
        public ObservableCollection<Direction> DirectionList { get; set; }
        public ObservableCollection<SoftwareEntity> SoftwareList { get; set; }
        public ObservableCollection<Inventory> InventoryList { get; set; }
        public ObservableCollection<InventoryItem> InventoryItemList { get; set; }
        public ObservableCollection<NetworkSetting> NetworkSettingList { get; set; }

        // Выбранные элементы
        private Equipment _selectedEquipment;
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

        public bool IsEquipmentSelected => SelectedEquipment != null;

        // Состояние
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // Поиск
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
            _authService = App.AuthService;
            _exportService = new ExportService();

            InitializeCollections();
        }

        private void InitializeCollections()
        {
            EquipmentList = new ObservableCollection<Equipment>();
            UserList = new ObservableCollection<User>();
            ClassroomList = new ObservableCollection<Classroom>();
            ConsumableList = new ObservableCollection<Consumable>();
            StatusList = new ObservableCollection<Status>();
            EquipmentTypeList = new ObservableCollection<EquipmentType>();
            ModelList = new ObservableCollection<ModelEntity>();
            ConsumableTypeList = new ObservableCollection<ConsumableType>();
            ConsumableCharacteristicList = new ObservableCollection<ConsumableCharacteristic>();
            DeveloperList = new ObservableCollection<Developer>();
            DirectionList = new ObservableCollection<Direction>();
            SoftwareList = new ObservableCollection<SoftwareEntity>();
            InventoryList = new ObservableCollection<Inventory>();
            InventoryItemList = new ObservableCollection<InventoryItem>();
            NetworkSettingList = new ObservableCollection<NetworkSetting>();
        }

        public async Task LoadAllDataAsync()
        {
            IsLoading = true;
            StatusMessage = "Загрузка данных...";

            try
            {
                await Task.WhenAll(
                    LoadEquipmentAsync(),
                    LoadUsersAsync(),
                    LoadClassroomsAsync(),
                    LoadConsumablesAsync(),
                    LoadStatusesAsync(),
                    LoadEquipmentTypesAsync(),
                    LoadModelsAsync(),
                    LoadConsumableTypesAsync(),
                    LoadConsumableCharacteristicsAsync(),
                    LoadDevelopersAsync(),
                    LoadDirectionsAsync(),
                    LoadSoftwareAsync(),
                    LoadInventoriesAsync(),
                    LoadInventoryItemsAsync(),
                    LoadNetworkSettingsAsync()
                );

                StatusMessage = $"Данные загружены: {EquipmentList.Count} оборудования, {UserList.Count} пользователей";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEquipmentAsync()
        {
            var items = await _cacheService.GetOrSetAsync("equipment_list",
                async () => await _apiService.GetListAsync<Equipment>("EquipmentController"));

            Application.Current.Dispatcher.Invoke(() =>
            {
                EquipmentList.Clear();
                if (items != null)
                {
                    foreach (var item in items) EquipmentList.Add(item);
                }
            });
        }

        private async Task LoadUsersAsync()
        {
            var items = await _cacheService.GetOrSetAsync("users_list",
                async () => await _apiService.GetListAsync<User>("UsersController"));

            Application.Current.Dispatcher.Invoke(() =>
            {
                UserList.Clear();
                if (items != null)
                {
                    foreach (var item in items) UserList.Add(item);
                }
            });
        }

        private async Task LoadClassroomsAsync()
        {
            var items = await _cacheService.GetOrSetAsync("classrooms_list",
                async () => await _apiService.GetListAsync<Classroom>("ClassroomsController"));

            Application.Current.Dispatcher.Invoke(() =>
            {
                ClassroomList.Clear();
                if (items != null)
                {
                    foreach (var item in items) ClassroomList.Add(item);
                }
            });
        }

        // Аналогичные методы для других сущностей...

        public async Task<bool> AddEquipmentAsync(Equipment equipment)
        {
            var errors = ValidationHelper.ValidateEquipment(equipment);
            if (errors.Count > 0)
            {
                MessageBox.Show(ValidationHelper.FormatValidationErrors(errors),
                    "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var success = await _apiService.AddItemAsync("EquipmentController", equipment);
            if (success)
            {
                _cacheService.Remove("equipment_list");
                await LoadEquipmentAsync();
                StatusMessage = "Оборудование добавлено";
                return true;
            }

            StatusMessage = "Ошибка добавления оборудования";
            return false;
        }

        public async Task<bool> UpdateEquipmentAsync(Equipment equipment)
        {
            if (equipment?.id == null || equipment.id <= 0)
            {
                MessageBox.Show("Неверный ID оборудования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var errors = ValidationHelper.ValidateEquipment(equipment);
            if (errors.Count > 0)
            {
                MessageBox.Show(ValidationHelper.FormatValidationErrors(errors),
                    "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var success = await _apiService.UpdateItemAsync("EquipmentController", equipment.id, equipment);
            if (success)
            {
                _cacheService.Remove("equipment_list");
                await LoadEquipmentAsync();
                StatusMessage = "Оборудование обновлено";
                return true;
            }

            StatusMessage = "Ошибка обновления оборудования";
            return false;
        }

        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            if (id <= 0)
            {
                MessageBox.Show("Неверный ID оборудования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить это оборудование?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return false;

            var success = await _apiService.DeleteItemAsync("EquipmentController", id);
            if (success)
            {
                _cacheService.Remove("equipment_list");
                await LoadEquipmentAsync();
                StatusMessage = "Оборудование удалено";
                return true;
            }

            StatusMessage = "Ошибка удаления оборудования";
            return false;
        }

        public void SearchEquipment(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return;
            }

            searchText = searchText.ToLower();
            var filtered = EquipmentList.Where(e =>
                (e.name?.ToLower().Contains(searchText) ?? false) ||
                (e.inventory_number?.ToLower().Contains(searchText) ?? false) ||
                (e.comment?.ToLower().Contains(searchText) ?? false));
        }

        public async Task ExportEquipmentToExcel()
        {
            await _exportService.ExportToExcel(EquipmentList, "Оборудование", "Оборудование");
            StatusMessage = "Экспорт завершен";
        }

        public async Task ExportUsersToExcel()
        {
            await _exportService.ExportToExcel(UserList, "Пользователи", "Пользователи");
            StatusMessage = "Экспорт завершен";
        }

        public bool CanEditEquipment()
        {
            return _authService.HasPermission("admin") || _authService.HasPermission("teacher");
        }

        public bool CanDeleteEquipment()
        {
            return _authService.HasPermission("admin");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}