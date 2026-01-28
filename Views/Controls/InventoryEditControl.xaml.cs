using AdminUP.Models;
using System;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class InventoryEditControl : BaseEditControl
    {
        private Inventory _inventory;

        public InventoryEditControl(Inventory inventory = null)
        {
            InitializeComponent();

            _inventory = inventory ?? new Inventory
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
            };

            DataContext = this;
        }

        public string Name
        {
            get => _inventory?.Name;
            set
            {
                if (_inventory != null)
                {
                    _inventory.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public DateTime StartDate
        {
            get => _inventory?.StartDate ?? DateTime.Now;
            set
            {
                if (_inventory != null)
                {
                    _inventory.StartDate = value;
                    RaisePropertyChanged(nameof(StartDate));
                }
            }
        }

        public DateTime EndDate
        {
            get => _inventory?.EndDate ?? DateTime.Now;
            set
            {
                if (_inventory != null)
                {
                    _inventory.EndDate = value;
                    RaisePropertyChanged(nameof(EndDate));
                }
            }
        }

        public Inventory GetInventory()
        {
            return _inventory;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_inventory.Name, "Название инвентаризации"))
                return false;

            if (_inventory.Name?.Length > 100)
                AddValidationError("Название инвентаризации не должно превышать 100 символов");

            if (_inventory.StartDate > _inventory.EndDate)
                AddValidationError("Дата начала не может быть позже даты окончания");

            if (_inventory.EndDate < _inventory.StartDate)
                AddValidationError("Дата окончания не может быть раньше даты начала");

            return !HasErrors;
        }
    }
}