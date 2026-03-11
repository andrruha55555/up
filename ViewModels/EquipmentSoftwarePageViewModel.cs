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
    public class EquipmentSoftwarePageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<EquipmentSoftware> _equipmentSoftwareList;
        private EquipmentSoftware _selectedEquipmentSoftware;
        private bool _isLoading;
        private string _searchText;
        private int? _selectedEquipmentId;
        private int? _selectedSoftwareId;

        public ObservableCollection<EquipmentSoftware> EquipmentSoftwareList
        {
            get => _equipmentSoftwareList;
            set
            {
                _equipmentSoftwareList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Equipment> EquipmentList { get; set; }
        public ObservableCollection<SoftwareEntity> SoftwareList { get; set; }

        public EquipmentSoftware SelectedEquipmentSoftware
        {
            get => _selectedEquipmentSoftware;
            set
            {
                _selectedEquipmentSoftware = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEquipmentSoftwareSelected));
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
            }
        }

        public int? SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set
            {
                _selectedEquipmentId = value;
                OnPropertyChanged();
                FilterEquipmentSoftware();
            }
        }

        public int? SelectedSoftwareId
        {
            get => _selectedSoftwareId;
            set
            {
                _selectedSoftwareId = value;
                OnPropertyChanged();
                FilterEquipmentSoftware();
            }
        }

        public bool IsEquipmentSoftwareSelected => SelectedEquipmentSoftware != null;

        public ObservableCollection<EquipmentSoftware> FilteredEquipmentSoftwareList { get; set; }

        public EquipmentSoftwarePageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            EquipmentSoftwareList = new ObservableCollection<EquipmentSoftware>();
            EquipmentList = new ObservableCollection<Equipment>();
            SoftwareList = new ObservableCollection<SoftwareEntity>();
            FilteredEquipmentSoftwareList = new ObservableCollection<EquipmentSoftware>();
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadEquipmentSoftwareAsync(),
                    LoadEquipmentAsync(),
                    LoadSoftwareAsync()
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEquipmentSoftwareAsync()
        {
            var equipmentSoftware = await _cacheService.GetOrSetAsync("equipment_software_page_list",
                async () => await _apiService.GetListAsync<EquipmentSoftware>("EquipmentSoftwareController"));

            // Не-админ видит только ПО своего оборудования
            bool isAdmin1 = App.AuthService.IsAdmin;
            var allEq1 = await _apiService.GetListAsync<Equipment>("EquipmentController");
            var myIds1 = isAdmin1
                ? null
                : (allEq1 ?? new())
                    .Where(e => e.responsible_user_id == App.AuthService.CurrentUserId ||
                                e.temp_responsible_user_id == App.AuthService.CurrentUserId)
                    .Select(e => e.id)
                    .ToHashSet();

            EquipmentSoftwareList.Clear();
            if (equipmentSoftware != null)
            {
                foreach (var item in equipmentSoftware)
                {
                    if (!isAdmin1 && !myIds1!.Contains(item.equipment_id)) continue;
                    EquipmentSoftwareList.Add(item);
                }
            }

            FilterEquipmentSoftware();
        }

        private async Task LoadEquipmentAsync()
        {
            var equipment = await _cacheService.GetOrSetAsync("equipment_for_software",
                async () => await _apiService.GetListAsync<Equipment>("EquipmentController"));

            bool isAdmin2 = App.AuthService.IsAdmin;
            int myId2 = App.AuthService.CurrentUserId;

            EquipmentList.Clear();
            if (equipment != null)
            {
                foreach (var item in equipment)
                {
                    if (!isAdmin2 &&
                        item.responsible_user_id != myId2 &&
                        item.temp_responsible_user_id != myId2) continue;
                    EquipmentList.Add(item);
                }
            }
        }

        private async Task LoadSoftwareAsync()
        {
            var software = await _cacheService.GetOrSetAsync("software_for_equipment",
                async () => await _apiService.GetListAsync<SoftwareEntity>("SoftwareController"));

            SoftwareList.Clear();
            if (software != null)
            {
                foreach (var item in software)
                {
                    SoftwareList.Add(item);
                }
            }
        }

        public void FilterEquipmentSoftware()
        {
            FilteredEquipmentSoftwareList.Clear();

            IEnumerable<EquipmentSoftware> filtered = EquipmentSoftwareList;

            if (SelectedEquipmentId.HasValue && SelectedEquipmentId > 0)
            {
                filtered = filtered.Where(es => es.equipment_id == SelectedEquipmentId.Value);
            }

            if (SelectedSoftwareId.HasValue && SelectedSoftwareId > 0)
            {
                filtered = filtered.Where(es => es.software_id == SelectedSoftwareId.Value);
            }

            foreach (var item in filtered)
            {
                FilteredEquipmentSoftwareList.Add(item);
            }
        }

        public async Task<bool> AddEquipmentSoftwareAsync(EquipmentSoftware equipmentSoftware)
        {
            try
            {
                var success = await _apiService.AddItemAsync("EquipmentSoftwareController", equipmentSoftware);
                if (success)
                {
                    _cacheService.Remove("equipment_software_page_list");
                    await LoadEquipmentSoftwareAsync();
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

        public async Task<bool> UpdateEquipmentSoftwareAsync(EquipmentSoftware equipmentSoftware)
        {
            try
            {
                // Для EquipmentSoftware нет метода Update в API, только Add и Delete
                // Поэтому сначала удаляем старую связь, потом добавляем новую
                var deleteSuccess = await _apiService.DeleteItemAsync("EquipmentSoftwareController", equipmentSoftware.equipment_id);
                if (deleteSuccess)
                {
                    var addSuccess = await _apiService.AddItemAsync("EquipmentSoftwareController", equipmentSoftware);
                    if (addSuccess)
                    {
                        _cacheService.Remove("equipment_software_page_list");
                        await LoadEquipmentSoftwareAsync();
                        return true;
                    }
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

        public async Task<bool> DeleteEquipmentSoftwareAsync(int equipmentId, int softwareId)
        {
            try
            {
                var success = await _apiService.DeleteItemAsync("EquipmentSoftwareController",
                    $"equipmentId={equipmentId}&softwareId={softwareId}");
                if (success)
                {
                    _cacheService.Remove("equipment_software_page_list");
                    await LoadEquipmentSoftwareAsync();
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