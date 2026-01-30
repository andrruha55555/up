using AdminUP.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class DeveloperEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Developer _developer;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public DeveloperEditControl(Developer developer = null)
        {
            InitializeComponent();
            _developer = developer ?? new Developer();
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
            get => _developer?.Name;
            set
            {
                if (_developer != null)
                {
                    _developer.Name = value;
                    RaisePropertyChanged(); 
                }
            }
        }

        public Developer GetDeveloper() => _developer;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_developer.Name, "Название разработчика"))
                return false;

            if (_developer.Name?.Length > 100)
                AddValidationError("Название разработчика не должно превышать 100 символов");

            return !HasErrors;
        }
    }
}
