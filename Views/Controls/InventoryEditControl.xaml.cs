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
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
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

        public string Name
        {
            get => _inventory?.Name;
            set
            {
                if (_inventory != null)
                {
                    _inventory.Name = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                }
            }
        }
        public Inventory GetInventory() => _inventory;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_inventory.Name, "Название инвентаризации"))
                return false;

            if (_inventory.Name?.Length > 100)
                AddValidationError("Название инвентаризации не должно превышать 100 символов");
            if (_inventory.StartDate > _inventory.EndDate)
                AddValidationError("Дата начала не может быть позже даты окончания");

            return !HasErrors;
        }
    }
}
