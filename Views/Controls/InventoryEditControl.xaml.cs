using AdminUP.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class InventoryEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Inventory _inventory;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public InventoryEditControl(Inventory inventory = null)
        {
            InitializeComponent();

            _inventory = inventory ?? new Inventory
            {
                start_date = DateTime.Now,
                end_date = DateTime.Now.AddDays(7)
            };

            DataContext = this;
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        private bool ValidateRequiredField(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddValidationError($"{fieldName} обязательно для заполнения");
                return false;
            }
            return true;
        }
        public bool Validate() => ValidateData();

        public string InventoryName
        {
            get => _inventory?.name;
            set
            {
                if (_inventory != null)
                {
                    _inventory.name = value;
                    RaisePropertyChanged();
                }
            }
        }
        public DateTime StartDate
        {
            get => _inventory?.start_date ?? DateTime.Now;
            set
            {
                if (_inventory != null)
                {
                    _inventory.start_date = value;
                    RaisePropertyChanged();
                }
            }
        }
        public DateTime EndDate
        {
            get => _inventory?.end_date ?? DateTime.Now;
            set
            {
                if (_inventory != null)
                {
                    _inventory.end_date = value;
                    RaisePropertyChanged();
                }
            }
        }
        public Inventory GetInventory() => _inventory;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_inventory.name, "Название инвентаризации"))
                return false;

            if (_inventory.name?.Length > 100)
                AddValidationError("Название инвентаризации не должно превышать 100 символов");
            if (_inventory.start_date > _inventory.end_date)
                AddValidationError("Дата начала не может быть позже даты окончания");

            return !HasErrors;
        }
    }
}
