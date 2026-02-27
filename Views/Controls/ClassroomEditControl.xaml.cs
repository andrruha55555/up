using AdminUP.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class ClassroomEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Classroom _classroom;

        public ObservableCollection<User> AvailableUsers { get; } = new();
        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ClassroomEditControl(Classroom classroom = null)
        {
            InitializeComponent();

            _classroom = classroom ?? new Classroom();
            DataContext = this;

            _ = LoadDataAsync();
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

        private async Task LoadDataAsync()
        {
            var users = await App.ApiService.GetListAsync<User>("UsersController");

            AvailableUsers.Clear();

            // "Не назначен" (id=0)
            AvailableUsers.Add(new User
            {
                id = 0,
                last_name = "Не",
                first_name = "назначен",
                middle_name = ""
            });

            if (users != null)
            {
                foreach (var u in users)
                    AvailableUsers.Add(u);
            }

            RaisePropertyChanged(nameof(AvailableUsers));
        }

        // ВАЖНО: НЕ использовать имя Name
        public string ClassroomName
        {
            get => _classroom?.name;
            set
            {
                if (_classroom != null)
                {
                    _classroom.name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ClassroomShortName
        {
            get => _classroom?.short_name;
            set
            {
                if (_classroom != null)
                {
                    _classroom.short_name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int? ResponsibleUserId
        {
            get => _classroom?.responsible_user_id;
            set
            {
                if (_classroom != null)
                {
                    _classroom.responsible_user_id = (value == 0) ? null : value;
                    RaisePropertyChanged();
                }
            }
        }

        public int? TempResponsibleUserId
        {
            get => _classroom?.temp_responsible_user_id;
            set
            {
                if (_classroom != null)
                {
                    _classroom.temp_responsible_user_id = (value == 0) ? null : value;
                    RaisePropertyChanged();
                }
            }
        }

        // Чтобы EditDialog.GetEditedItem() работал как в Users
        public object GetEditedItem() => _classroom;

        public Classroom GetClassroom() => _classroom;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_classroom.name, "Название аудитории"))
                return false;

            if (!ValidateRequiredField(_classroom.short_name, "Сокращение"))
                return false;

            if (_classroom.short_name?.Length > 20)
                AddValidationError("Сокращение не должно превышать 20 символов");

            if (_classroom.short_name?.All(char.IsDigit) == false)
                AddValidationError("Сокращение должно содержать только цифры");

            return !HasErrors;
        }
    }
}