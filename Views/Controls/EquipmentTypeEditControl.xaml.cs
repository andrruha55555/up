using AdminUP.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentTypeEditControl : UserControl, INotifyPropertyChanged
    {
        private readonly EquipmentType _equipmentType;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public EquipmentTypeEditControl(EquipmentType equipmentType = null)
        {
            InitializeComponent();
            _equipmentType = equipmentType ?? new EquipmentType();
            DataContext = this;
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearValidationErrors()
        {
            ValidationErrors.Clear();
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(HasErrors));
        }

        private void AddValidationError(string message)
        {
            ValidationErrors.Add(message);
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(HasErrors));
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
            get => _equipmentType?.Name;
            set
            {
                if (_equipmentType != null)
                {
                    _equipmentType.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public EquipmentType GetEquipmentType() => _equipmentType;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_equipmentType?.Name, "Тип оборудования"))
                return false;

            if (_equipmentType.Name?.Length > 50)
                AddValidationError("Тип оборудования не должен превышать 50 символов");

            return !HasErrors;
        }
    }
}
