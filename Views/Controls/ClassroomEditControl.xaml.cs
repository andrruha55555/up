using AdminUP.Helpers;
using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для ClassroomEditControl.xaml
    /// </summary>
    public partial class ClassroomEditControl : BaseEditControl
    {
        private Classroom _classroom;
        private ApiService _apiService;
        public ObservableCollection<User> AvailableUsers { get; set; }

        public Classroom Classroom
        {
            get => _classroom;
            set
            {
                _classroom = value;
                RaisePropertyChanged(nameof(Classroom));
            }
        }

        public string Name
        {
            get => _classroom?.Name;
            set
            {
                if (_classroom != null)
                {
                    _classroom.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public string ShortName
        {
            get => _classroom?.ShortName;
            set
            {
                if (_classroom != null)
                {
                    _classroom.ShortName = value;
                    RaisePropertyChanged(nameof(ShortName));
                }
            }
        }

        public int? ResponsibleUserId
        {
            get => _classroom?.ResponsibleUserId;
            set
            {
                if (_classroom != null)
                {
                    _classroom.ResponsibleUserId = value;
                    RaisePropertyChanged(nameof(ResponsibleUserId));
                }
            }
        }

        public int? TempResponsibleUserId
        {
            get => _classroom?.TempResponsibleUserId;
            set
            {
                if (_classroom != null)
                {
                    _classroom.TempResponsibleUserId = value;
                    RaisePropertyChanged(nameof(TempResponsibleUserId));
                }
            }
        }

        public ClassroomEditControl(Classroom classroom = null)
        {
            InitializeComponent();

            _classroom = classroom ?? new Classroom();
            _apiService = new ApiService();
            AvailableUsers = new ObservableCollection<User>();

            DataContext = this;
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var users = await _apiService.GetListAsync<User>("UsersController");
            if (users != null)
            {
                AvailableUsers.Clear();
                foreach (var user in users)
                {
                    AvailableUsers.Add(user);
                }
                RaisePropertyChanged(nameof(AvailableUsers));
            }
        }

        public Classroom GetClassroom()
        {
            return _classroom;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_classroom.Name, "Название аудитории"))
                return false;

            if (!ValidateRequiredField(_classroom.ShortName, "Сокращение"))
                return false;

            if (_classroom.ShortName?.Length > 20)
                AddValidationError("Сокращение не должно превышать 20 символов");

            // Проверка уникальности (в идеале - через API)
            if (_classroom.ShortName?.All(char.IsDigit) == false)
                AddValidationError("Сокращение должно содержать только цифры");

            return !HasErrors;
        }
    }
}
