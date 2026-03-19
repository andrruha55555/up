
using AdminUP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public class EquipmentSelectItem : INotifyPropertyChanged
    {
        public int EquipmentId { get; }
        public string EquipmentName { get; }
        public string InventoryNumber { get; }
        public string ClassroomName { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPC(); }
        }

        public EquipmentSelectItem(Equipment eq, string classroomName, bool selected = false)
        {
            EquipmentId = eq.id;
            EquipmentName = eq.name ?? $"ID {eq.id}";
            InventoryNumber = eq.inventory_number ?? "";
            ClassroomName = classroomName;
            IsSelected = selected;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPC([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public partial class InventoryEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private Inventory _inventory;

        public ObservableCollection<EquipmentSelectItem> EquipmentItems { get; } = new();
        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        private int _selectedEquipmentCount;
        public int SelectedEquipmentCount
        {
            get => _selectedEquipmentCount;
            private set { _selectedEquipmentCount = value; Raise(); }
        }

        public InventoryEditControl(Inventory? inventory = null)
        {
            InitializeComponent();
            _inventory = inventory ?? new Inventory
            {
                start_date = DateTime.Now,
                end_date = DateTime.Now.AddDays(7)
            };
            DataContext = this;
            _ = LoadEquipmentAsync();
        }

        private async Task LoadEquipmentAsync()
        {
            try
            {
                var equipment = await App.ApiService.GetListAsync<Equipment>("EquipmentController")
                                 ?? new List<Equipment>();
                var classrooms = await App.ApiService.GetListAsync<Classroom>("ClassroomsController")
                                 ?? new List<Classroom>();
                var roomMap = classrooms.ToDictionary(c => c.id, c => c.name ?? c.short_name ?? "");

                // Если редактируем — загружаем уже добавленное оборудование
                HashSet<int> alreadySelected = new();
                if (_inventory.id > 0)
                {
                    var existing = await App.ApiService.GetListAsync<InventoryItem>("InventoryItemsController")
                                   ?? new List<InventoryItem>();
                    alreadySelected = existing
                        .Where(i => i.inventory_id == _inventory.id)
                        .Select(i => i.equipment_id)
                        .ToHashSet();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    EquipmentItems.Clear();
                    foreach (var eq in equipment.OrderBy(e => e.name))
                    {
                        string room = eq.classroom_id.HasValue && roomMap.TryGetValue(eq.classroom_id.Value, out var r) ? r : "";
                        var item = new EquipmentSelectItem(eq, room, alreadySelected.Contains(eq.id));
                        item.PropertyChanged += (_, e) =>
                        {
                            if (e.PropertyName == nameof(EquipmentSelectItem.IsSelected))
                                UpdateCount();
                        };
                        EquipmentItems.Add(item);
                    }
                    UpdateCount();
                });
            }
            catch { /* грузим без оборудования если ошибка */ }
        }

        private void UpdateCount()
            => SelectedEquipmentCount = EquipmentItems.Count(i => i.IsSelected);

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in EquipmentItems) i.IsSelected = true;
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in EquipmentItems) i.IsSelected = false;
        }

        // Свойства для биндинга
        public string? InventoryName
        {
            get => _inventory.name;
            set { _inventory.name = value; Raise(); }
        }
        public DateTime StartDate
        {
            get => _inventory.start_date;
            set { _inventory.start_date = value; Raise(); }
        }
        public DateTime EndDate
        {
            get => _inventory.end_date;
            set { _inventory.end_date = value; Raise(); }
        }

        public Inventory GetInventory() => _inventory;

        /// <summary>Список id оборудования которое выбрано для инвентаризации</summary>
        public List<int> GetSelectedEquipmentIds()
            => EquipmentItems.Where(i => i.IsSelected).Select(i => i.EquipmentId).ToList();

        public bool Validate()
        {
            ValidationErrors.Clear();
            if (string.IsNullOrWhiteSpace(_inventory.name))
                ValidationErrors.Add("Введите наименование инвентаризации.");
            if (_inventory.start_date > _inventory.end_date)
                ValidationErrors.Add("Дата начала не может быть позже даты окончания.");
            if (SelectedEquipmentCount == 0)
                ValidationErrors.Add("Выберите хотя бы одну единицу оборудования.");
            Raise(nameof(ValidationErrors));
            Raise(nameof(HasErrors));
            return !HasErrors;
        }

        private void Raise([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
