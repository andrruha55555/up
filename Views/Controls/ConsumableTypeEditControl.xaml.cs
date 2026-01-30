using AdminUP.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class ConsumableTypeEditControl : UserControl, INotifyPropertyChanged
    {
        private ConsumableType _consumableType;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ConsumableTypeEditControl(ConsumableType consumableType = null)
        {
            InitializeComponent();
            _consumableType = consumableType ?? new ConsumableType();
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
            get => _consumableType?.Name;
            set
            {
                if (_consumableType != null)
                {
                    _consumableType.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public ConsumableType GetConsumableType() => _consumableType;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_consumableType.Name, "Тип расходника"))
                return false;

            if (_consumableType.Name?.Length > 100)
                AddValidationError("Тип расходника не должен превышать 100 символов");

            return !HasErrors;
        }
    }
}
