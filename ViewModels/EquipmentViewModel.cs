using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdminUP.ViewModels
{
    public class EquipmentViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Equipment> _equipmentList;
        private Equipment _selectedEquipment;
        private bool _isLoading;

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

        public bool IsEquipmentSelected => SelectedEquipment != null;

        public EquipmentViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
            EquipmentList = new ObservableCollection<Equipment>();
        }

        public async Task LoadEquipmentAsync()
        {
            IsLoading = true;
            try
            {
                var equipment = await _cacheService.GetOrSetAsync("equipment_all",
                    async () => await _apiService.GetListAsync<Equipment>("EquipmentController"));

                EquipmentList.Clear();
                foreach (var item in equipment)
                {
                    EquipmentList.Add(item);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> AddEquipmentAsync(Equipment equipment)
        {
            var success = await _apiService.AddItemAsync("EquipmentController", equipment);
            if (success)
            {
                _cacheService.Remove("equipment_all"); // Инвалидируем кэш
                await LoadEquipmentAsync();
            }
            return success;
        }

        public async Task<bool> UpdateEquipmentAsync(int id, Equipment equipment)
        {
            var success = await _apiService.UpdateItemAsync("EquipmentController", id, equipment);
            if (success)
            {
                _cacheService.Remove("equipment_all");
                await LoadEquipmentAsync();
            }
            return success;
        }

        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            var success = await _apiService.DeleteItemAsync("EquipmentController", id);
            if (success)
            {
                _cacheService.Remove("equipment_all");
                await LoadEquipmentAsync();
            }
            return success;
        }

        public void SearchEquipment(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return;
            }

            var filtered = EquipmentList.Where(e =>
                e.Name?.ToLower().Contains(searchText.ToLower()) == true ||
                e.InventoryNumber?.ToLower().Contains(searchText.ToLower()) == true ||
                e.Comment?.ToLower().Contains(searchText.ToLower()) == true);

            // Можно обновить коллекцию или использовать отдельную коллекцию для отображения
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
