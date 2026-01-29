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
        public ObservableCollection<Software> SoftwareList { get; set; }

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
            SoftwareList = new ObservableCollection<Software>();
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

            EquipmentSoftwareList.Clear();
            if (equipmentSoftware != null)
            {
                foreach (var item in equipmentSoftware)
                {
                    EquipmentSoftwareList.Add(item);
                }
            }

            FilterEquipmentSoftware();
        }

        private async Task LoadEquipmentAsync()
        {
            var equipment = await _cacheService.GetOrSetAsync("equipment_for_software",
                async () => await _apiService.GetListAsync<Equipment>("EquipmentController"));

            EquipmentList.Clear();
            if (equipment != null)
            {
                foreach (var item in equipment)
                {
                    EquipmentList.Add(item);
                }
            }
        }

        private async Task LoadSoftwareAsync()
        {
            var software = await _cacheService.GetOrSetAsync("software_for_equipment",
                async () => await _apiService.GetListAsync<Software>("SoftwareController"));

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
                filtered = filtered.Where(es => es.EquipmentId == SelectedEquipmentId.Value);
            }

            if (SelectedSoftwareId.HasValue && SelectedSoftwareId > 0)
            {
                filtered = filtered.Where(es => es.SoftwareId == SelectedSoftwareId.Value);
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
                var deleteSuccess = await _apiService.DeleteItemAsync("EquipmentSoftwareController", equipmentSoftware.EquipmentId);
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
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту связь?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                // В API нет метода Delete для EquipmentSoftware по equipment_id и software_id
                // Нужно реализовать свой метод или использовать другой подход
                MessageBox.Show("Удаление связей пока не реализовано в API", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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