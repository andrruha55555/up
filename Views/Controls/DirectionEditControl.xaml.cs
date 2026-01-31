using AdminUP.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class DirectionEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Direction _direction;

        // Валидация (как у тебя в XAML: ItemsControl ItemsSource="{Binding ValidationErrors}")
        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public DirectionEditControl(Direction direction = null)
        {
            InitializeComponent();

            _direction = direction ?? new Direction();

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

        // чтобы диалог/страница могла вызвать валидацию
        public bool Validate() => ValidateData();

        public string Name
        {
            get => _direction?.name;
            set
            {
                if (_direction != null)
                {
                    _direction.name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Direction GetDirection() => _direction;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_direction.name, "Название направления"))
                return false;

            if (_direction.name?.Length > 100)
                AddValidationError("Название направления не должно превышать 100 символов");

            return !HasErrors;
        }
    }
}
