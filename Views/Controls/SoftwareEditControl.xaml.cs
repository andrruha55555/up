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
        private readonly Software _software;
        private readonly ApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ObservableCollection<Developer> AvailableDevelopers { get; } = new();

        public SoftwareEditControl(Software software = null)
        {
            InitializeComponent();

            _software = software ?? new Software();
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

            if (!ValidateRequiredField(_software.Name, "Название ПО"))
                return false;

            if (_software.Name?.Length > 100)
                AddValidationError("Название ПО не должно превышать 100 символов");

            if (_software.Version?.Length > 50)
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
            get => _software?.Name;
            set
            {
                if (_software != null)
                {
                    _software.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public int? DeveloperId
        {
            get => _software?.DeveloperId;
            set
            {
                if (_software != null)
                {
                    _software.DeveloperId = value;
                    RaisePropertyChanged(nameof(DeveloperId));
                }
            }
        }

        public string Version
        {
            get => _software?.Version;
            set
            {
                if (_software != null)
                {
                    _software.Version = value;
                    RaisePropertyChanged(nameof(Version));
                }
            }
        }

        public Software GetSoftware() => _software;
    }
}
