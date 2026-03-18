
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
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;
        public ObservableCollection<Inventory> AvailableInventories { get; set; } = new();
        public ObservableCollection<Equipment> AvailableEquipment { get; set; } = new();
        public ObservableCollection<User> AvailableUsers { get; set; } = new();

        public InventoryItemEditControl(InventoryItem? inventoryItem = null)
        {
            InitializeComponent();
            _inventoryItem = inventoryItem ?? new InventoryItem
            {
                checked_at = DateTime.Now,
                // По умолчанию — текущий авторизованный пользователь
                checked_by_user_id = App.AuthService.CurrentUserId
            };
            DataContext = this;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await Task.WhenAll(LoadInventoriesAsync(), LoadEquipmentAsync(), LoadUsersAsync());
        }

        private async Task LoadInventoriesAsync()
        {
            var list = await App.ApiService.GetListAsync<Inventory>("InventoriesController");
            AvailableInventories.Clear();
            if (list != null) foreach (var x in list) AvailableInventories.Add(x);
            RaisePropertyChanged(nameof(AvailableInventories));
        }

        private async Task LoadEquipmentAsync()
        {
            var list = await App.ApiService.GetListAsync<Equipment>("EquipmentController");
            AvailableEquipment.Clear();
            if (list != null) foreach (var x in list) AvailableEquipment.Add(x);
            RaisePropertyChanged(nameof(AvailableEquipment));
        }

        private async Task LoadUsersAsync()
        {
            var list = await App.ApiService.GetListAsync<User>("UsersController");
            AvailableUsers.Clear();
            if (list != null) foreach (var x in list) AvailableUsers.Add(x);
            RaisePropertyChanged(nameof(AvailableUsers));
        }

        public int InventoryId
        {
            get => _inventoryItem?.inventory_id ?? 0;
            set { if (_inventoryItem != null) { _inventoryItem.inventory_id = value; RaisePropertyChanged(); } }
        }

        public int EquipmentId
        {
            get => _inventoryItem?.equipment_id ?? 0;
            set { if (_inventoryItem != null) { _inventoryItem.equipment_id = value; RaisePropertyChanged(); } }
        }

        public int? CheckedByUserId
        {
            get => _inventoryItem?.checked_by_user_id;
            set { if (_inventoryItem != null) { _inventoryItem.checked_by_user_id = value; RaisePropertyChanged(); } }
        }

        public DateTime CheckedAt
        {
            get => _inventoryItem?.checked_at ?? DateTime.Now;
            set { if (_inventoryItem != null) { _inventoryItem.checked_at = value; RaisePropertyChanged(); } }
        }

        public string? Comment
        {
            get => _inventoryItem?.comment;
            set { if (_inventoryItem != null) { _inventoryItem.comment = value; RaisePropertyChanged(); } }
        }

        public InventoryItem GetInventoryItem() => _inventoryItem;

        public bool Validate()
        {
            ValidationErrors.Clear();
            if (_inventoryItem.inventory_id <= 0)
                ValidationErrors.Add("Выберите инвентаризацию.");
            if (_inventoryItem.equipment_id <= 0)
                ValidationErrors.Add("Выберите оборудование.");
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(HasErrors));
            return !HasErrors;
        }

        private void RaisePropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
