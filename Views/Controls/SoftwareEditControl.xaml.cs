using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class SoftwareEditControl : UserControl, INotifyPropertyChanged
    {
        private readonly SoftwareEntity _software;
        private readonly ApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ObservableCollection<Developer> AvailableDevelopers { get; } = new();

        public SoftwareEditControl(SoftwareEntity software = null)
        {
            InitializeComponent();

            _software = software ?? new SoftwareEntity();
            _apiService = new ApiService();

            DataContext = this;

            _ = LoadDevelopersAsync();
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

        public bool Validate()
        {
            return ValidateData();
        }

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_software.name, "Название ПО"))
                return false;

            if (_software.name?.Length > 100)
                AddValidationError("Название ПО не должно превышать 100 символов");

            if (_software.version?.Length > 50)
                AddValidationError("Версия не должна превышать 50 символов");

            return !HasErrors;
        }

        private async Task LoadDevelopersAsync()
        {
            var developers = await _apiService.GetListAsync<Developer>("DevelopersController");
            if (developers != null)
            {
                AvailableDevelopers.Clear();
                foreach (var developer in developers)
                    AvailableDevelopers.Add(developer);

                RaisePropertyChanged(nameof(AvailableDevelopers));
            }
        }

        public string Name
        {
            get => _software?.name;
            set
            {
                if (_software != null)
                {
                    _software.name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public int? DeveloperId
        {
            get => _software?.developer_id;
            set
            {
                if (_software != null)
                {
                    _software.developer_id = value;
                    RaisePropertyChanged(nameof(DeveloperId));
                }
            }
        }

        public string Version
        {
            get => _software?.version;
            set
            {
                if (_software != null)
                {
                    _software.version = value;
                    RaisePropertyChanged(nameof(Version));
                }
            }
        }

        public SoftwareEntity GetSoftware() => _software;
    }
}
