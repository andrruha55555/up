using AdminUP.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class StatusEditControl : UserControl, INotifyPropertyChanged
    {
        private readonly Status _status;

        public ObservableCollection<string> ValidationErrors { get; } = new();

        public bool HasErrors => ValidationErrors.Count > 0;

        public StatusEditControl(Status? status = null)
        {
            InitializeComponent();
            _status = status ?? new Status();
            DataContext = this;
        }

        public string? Name
        {
            get => _status.Name;
            set
            {
                _status.Name = value;
                RaisePropertyChanged();
            }
        }

        public Status GetStatus() => _status;
        public bool Validate()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_status.Name, "Название статуса"))
                return false;

            if (_status.Name?.Length > 50)
                AddValidationError("Название статуса не должно превышать 50 символов");

            return !HasErrors;
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

        private void ClearValidationErrors() => ValidationErrors.Clear();

        private void AddValidationError(string message) => ValidationErrors.Add(message);
        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
