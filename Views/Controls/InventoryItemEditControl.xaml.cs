
using AdminUP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public class EquipmentCheckItem : INotifyPropertyChanged
    {
        public int EquipmentId { get; }
        public string EquipmentName { get; }
        public string InventoryNumber { get; }
        public int? ExistingItemId { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPC(); }
        }

        private string _comment = "";
        public string Comment
        {
            get => _comment;
            set { _comment = value ?? ""; OnPC(); }
        }

        public EquipmentCheckItem(Equipment eq, bool isChecked = false, string? comment = null, int? existingId = null)
        {
            EquipmentId = eq.id;
            EquipmentName = eq.name ?? $"ID {eq.id}";
            InventoryNumber = eq.inventory_number ?? "";
            IsChecked = isChecked;
            Comment = comment ?? "";
            ExistingItemId = existingId;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPC([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public partial class InventoryItemEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _inventoryId;
        private int _checkedByUserId;

        public ObservableCollection<EquipmentCheckItem> EquipmentItems { get; } = new();
        public ObservableCollection<EquipmentCheckItem> FilteredItems { get; } = new();

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; Raise(); FilterItems(); }
        }

        private int _checkedCount;
        public int CheckedCount
        {
            get => _checkedCount;
            private set { _checkedCount = value; Raise(); }
        }

        public InventoryItemEditControl(int inventoryId, int checkedByUserId,
            List<Equipment> allEquipment,
            List<InventoryItem> existingItems)
        {
            InitializeComponent();
            _inventoryId = inventoryId;
            _checkedByUserId = checkedByUserId;
            DataContext = this;

            // existingMap: equipment_id -> existing InventoryItem
            var existingMap = existingItems.ToDictionary(i => i.equipment_id, i => i);

            foreach (var eq in allEquipment.OrderBy(e => e.name))
            {
                bool alreadyChecked = existingMap.TryGetValue(eq.id, out var existing)
                                      && existing.checked_by_user_id.HasValue;
                var item = new EquipmentCheckItem(
                    eq,
                    isChecked: alreadyChecked,
                    comment: existing?.comment,
                    existingId: existing?.id
                );
                item.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(EquipmentCheckItem.IsChecked))
                        UpdateCount();
                };
                EquipmentItems.Add(item);
            }
            FilterItems();
            UpdateCount();
        }

        private void FilterItems()
        {
            var q = _searchText.Trim().ToLowerInvariant();
            FilteredItems.Clear();
            foreach (var item in EquipmentItems)
            {
                if (string.IsNullOrWhiteSpace(q) ||
                    item.EquipmentName.ToLowerInvariant().Contains(q) ||
                    item.InventoryNumber.ToLowerInvariant().Contains(q))
                    FilteredItems.Add(item);
            }
        }

        private void UpdateCount()
            => CheckedCount = EquipmentItems.Count(i => i.IsChecked);

        private void SelectAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var i in FilteredItems) i.IsChecked = true;
        }

        private void DeselectAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var i in FilteredItems) i.IsChecked = false;
        }

        /// <summary>
        /// toAdd    = новые отметки (галочка поставлена, записи не было)
        /// toUpdate = обновление (галочка была и есть или комментарий изменился)  
        /// toUncheck = снятые галочки (запись есть, но checked_by = null теперь)
        /// </summary>
        public (List<InventoryItem> toAdd,
                List<InventoryItem> toUpdate,
                List<InventoryItem> toUncheck) GetChanges()
        {
            var toAdd = new List<InventoryItem>();
            var toUpdate = new List<InventoryItem>();
            var toUncheck = new List<InventoryItem>();
            var now = DateTime.Now;

            foreach (var eq in EquipmentItems)
            {
                if (eq.IsChecked && !eq.ExistingItemId.HasValue)
                {
                    toAdd.Add(new InventoryItem
                    {
                        inventory_id = _inventoryId,
                        equipment_id = eq.EquipmentId,
                        checked_by_user_id = _checkedByUserId,
                        checked_at = now,
                        comment = string.IsNullOrWhiteSpace(eq.Comment) ? null : eq.Comment
                    });
                }
                else if (eq.IsChecked && eq.ExistingItemId.HasValue)
                {
                    toUpdate.Add(new InventoryItem
                    {
                        id = eq.ExistingItemId.Value,
                        inventory_id = _inventoryId,
                        equipment_id = eq.EquipmentId,
                        checked_by_user_id = _checkedByUserId,
                        checked_at = now,
                        comment = string.IsNullOrWhiteSpace(eq.Comment) ? null : eq.Comment
                    });
                }
                else if (!eq.IsChecked && eq.ExistingItemId.HasValue)
                {
                    // Снята галочка — обнуляем проверку, но запись оставляем
                    toUncheck.Add(new InventoryItem
                    {
                        id = eq.ExistingItemId.Value,
                        inventory_id = _inventoryId,
                        equipment_id = eq.EquipmentId,
                        checked_by_user_id = null,
                        checked_at = null,
                        comment = null
                    });
                }
            }
            return (toAdd, toUpdate, toUncheck);
        }

        public bool Validate() => true;

        private void Raise([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
