using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class InventoryItemEditControl : UserControl, INotifyPropertyChanged
    {
        private InventoryItem _inventoryItem;
        private readonly ApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ObservableCollection<Inventory> AvailableInventories { get; set; }
        public ObservableCollection<Equipment> AvailableEquipment { get; set; }
        public ObservableCollection<User> AvailableUsers { get; set; }

        public InventoryItemEditControl(InventoryItem inventoryItem = null)
        {
            InitializeComponent();

            _inventoryItem = inventoryItem ?? new InventoryItem
            {
                CheckedAt = DateTime.Now
            };

            _apiService = new ApiService();
            AvailableInventories = new ObservableCollection<Inventory>();
            AvailableEquipment = new ObservableCollection<Equipment>();
            AvailableUsers = new ObservableCollection<User>();

            DataContext = this;

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await Task.WhenAll(
                LoadInventoriesAsync(),
                LoadEquipmentAsync(),
                LoadUsersAsync()
            );
        }

        private async Task LoadInventoriesAsync()
        {
            var inventories = await _apiService.GetListAsync<Inventory>("InventoriesController");
            if (inventories != null)
            {
                AvailableInventories.Clear();
                foreach (var inventory in inventories)
                    AvailableInventories.Add(inventory);

                RaisePropertyChanged(nameof(AvailableInventories));
            }
        }

        private async Task LoadEquipmentAsync()
        {
            var equipment = await _apiService.GetListAsync<Equipment>("EquipmentController");
            if (equipment != null)
            {
                AvailableEquipment.Clear();
                foreach (var item in equipment)
                    AvailableEquipment.Add(item);

                RaisePropertyChanged(nameof(AvailableEquipment));
            }
        }

        private async Task LoadUsersAsync()
        {
            var users = await _apiService.GetListAsync<User>("UsersController");
            if (users != null)
            {
                AvailableUsers.Clear();
                foreach (var user in users)
                    AvailableUsers.Add(user);

                RaisePropertyChanged(nameof(AvailableUsers));
            }
        }

        public int InventoryId
        {
            get => _inventoryItem?.InventoryId ?? 0;
            set
            {
                if (_inventoryItem != null)
                {
                    _inventoryItem.InventoryId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int EquipmentId
        {
            get => _inventoryItem?.EquipmentId ?? 0;
            set
            {
                if (_inventoryItem != null)
                {
                    _inventoryItem.EquipmentId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int? CheckedByUserId
        {
            get => _inventoryItem?.CheckedByUserId;
            set
            {
                if (_inventoryItem != null)
                {
                    _inventoryItem.CheckedByUserId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Comment
        {
            get => _inventoryItem?.Comment;
            set
            {
                if (_inventoryItem != null)
                {
                    _inventoryItem.Comment = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DateTime CheckedAt
        {
            get => _inventoryItem?.CheckedAt ?? DateTime.Now;
            set
            {
                if (_inventoryItem != null)
                {
                    _inventoryItem.CheckedAt = value;
                    RaisePropertyChanged();
                }
            }
        }

        public InventoryItem GetInventoryItem() => _inventoryItem;

        // ===== Валидация (аналог BaseEditControl, но внутри этого файла) =====

        private void RaisePropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void ClearValidationErrors()
        {
            ValidationErrors.Clear();
            RaisePropertyChanged(nameof(HasErrors));
            RaisePropertyChanged(nameof(ValidationErrors));
        }

        private void AddValidationError(string message)
        {
            ValidationErrors.Add(message);
            RaisePropertyChanged(nameof(HasErrors));
            RaisePropertyChanged(nameof(ValidationErrors));
        }

        public bool Validate()
        {
            ClearValidationErrors();

            if (_inventoryItem.InventoryId <= 0)
                AddValidationError("Выберите инвентаризацию");

            if (_inventoryItem.EquipmentId <= 0)
                AddValidationError("Выберите оборудование");

            return !HasErrors;
        }
    }
}
