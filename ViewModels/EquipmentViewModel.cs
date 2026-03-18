using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.Generic;
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
        private EquipmentRow _selectedEquipment;
        private bool _isLoading;
        private string _searchText;

        // Lookup-словари для отображения имён вместо ID
        public Dictionary<int, string> ClassroomNames { get; private set; } = new();
        public Dictionary<int, string> UserNames { get; private set; } = new();
        public Dictionary<int, string> StatusNames { get; private set; } = new();
        public Dictionary<int, string> ModelNames { get; private set; } = new();

        public ObservableCollection<Equipment> EquipmentList
        {
            get => _equipmentList;
            set
            {
                _equipmentList = value;
                OnPropertyChanged();
            }
        }

        public EquipmentRow SelectedEquipment
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

        public ObservableCollection<EquipmentRow> FilteredEquipmentList { get; set; }
        private List<EquipmentRow> _allRows = new();

        public EquipmentPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            EquipmentList = new ObservableCollection<Equipment>();
            FilteredEquipmentList = new ObservableCollection<EquipmentRow>();
        }

        public async Task LoadEquipmentAsync()
        {
            IsLoading = true;
            try
            {
                // Загрузка справочников параллельно
                var classroomsTask = _apiService.GetListAsync<Classroom>("ClassroomsController");
                var usersTask = _apiService.GetListAsync<User>("UsersController");
                var statusesTask = _apiService.GetListAsync<Status>("StatusesController");
                var modelsTask = _apiService.GetListAsync<ModelEntity>("ModelsController");
                var equipmentTask = _cacheService.GetOrSetAsync("equipment_page_list",
                    async () => await _apiService.GetListAsync<Equipment>("EquipmentController"));

                await Task.WhenAll(classroomsTask, usersTask, statusesTask, modelsTask, equipmentTask);

                ClassroomNames = (classroomsTask.Result ?? new())
                    .ToDictionary(c => c.id, c => c.name ?? c.short_name ?? c.id.ToString());
                UserNames = (usersTask.Result ?? new())
                    .ToDictionary(u => u.id, u => u.FullName);
                StatusNames = (statusesTask.Result ?? new())
                    .ToDictionary(s => s.id, s => s.name ?? s.id.ToString());
                ModelNames = (modelsTask.Result ?? new())
                    .ToDictionary(m => m.id, m => m.name ?? m.id.ToString());

                var equipment = equipmentTask.Result;
                EquipmentList.Clear();
                _allRows.Clear();

                // Фильтрация по ответственному: не-админ видит только своё оборудование
                bool isAdmin = App.AuthService.IsAdmin;
                int myId = App.AuthService.CurrentUserId;

                if (equipment != null)
                {
                    foreach (var item in equipment)
                    {
                        // Не-админ видит оборудование где он responsible или temp_responsible
                        if (!isAdmin &&
                            item.responsible_user_id != myId &&
                            item.temp_responsible_user_id != myId)
                            continue;

                        EquipmentList.Add(item);
                        _allRows.Add(new EquipmentRow(item, ClassroomNames, UserNames, StatusNames, ModelNames));
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

            IEnumerable<EquipmentRow> source = _allRows;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.ToLower();
                source = source.Where(r =>
                    (r.Equipment.name?.ToLower().Contains(s) ?? false) ||
                    (r.Equipment.inventory_number?.ToLower().Contains(s) ?? false) ||
                    (r.ClassroomName?.ToLower().Contains(s) ?? false) ||
                    (r.StatusName?.ToLower().Contains(s) ?? false) ||
                    (r.Equipment.comment?.ToLower().Contains(s) ?? false));
            }

            foreach (var row in source)
                FilteredEquipmentList.Add(row);
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

    /// <summary>
    /// Строка таблицы оборудования с разрешёнными именами вместо FK-ID
    /// </summary>
    public class EquipmentRow
    {
        public Equipment Equipment { get; }

        public int Id => Equipment.id;
        public string Name => Equipment.name;
        public string InventoryNumber => Equipment.inventory_number;
        public string ClassroomName { get; }
        public string ResponsibleUser { get; }
        public string Cost => Equipment.cost.HasValue ? Equipment.cost.Value.ToString("N2") : "—";
        public string StatusName { get; }
        public string ModelName { get; }
        public string? ImagePath => Equipment.image_path;

        public EquipmentRow(Equipment eq,
            Dictionary<int, string> classrooms,
            Dictionary<int, string> users,
            Dictionary<int, string> statuses,
            Dictionary<int, string> models)
        {
            Equipment = eq;
            ClassroomName = eq.classroom_id.HasValue && classrooms.TryGetValue(eq.classroom_id.Value, out var c) ? c : "—";
            ResponsibleUser = eq.responsible_user_id.HasValue && users.TryGetValue(eq.responsible_user_id.Value, out var u) ? u : "—";
            StatusName = statuses.TryGetValue(eq.status_id, out var s) ? s : eq.status_id.ToString();
            ModelName = eq.model_id.HasValue && models.TryGetValue(eq.model_id.Value, out var m) ? m : "—";
        }
    }
}
