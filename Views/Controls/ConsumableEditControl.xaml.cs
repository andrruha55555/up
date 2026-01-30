using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class ConsumableEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Consumable _consumable;
        private readonly ApiService _apiService;

        public ObservableCollection<ConsumableType> AvailableConsumableTypes { get; private set; } = new();

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ConsumableEditControl(Consumable consumable = null)
        {
            InitializeComponent();

            _consumable = consumable ?? new Consumable();
            _apiService = new ApiService();

            DataContext = this;

            _ = LoadTypesAsync();
        }

        // ===== Helpers =====
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

        // ===== Data load =====
        private async Task LoadTypesAsync()
        {
            var types = await _apiService.GetListAsync<ConsumableType>("ConsumableTypesController");
            if (types != null)
            {
                AvailableConsumableTypes.Clear();
                foreach (var t in types)
                    AvailableConsumableTypes.Add(t);

                RaisePropertyChanged(nameof(AvailableConsumableTypes));
            }
        }

        // ===== Bindings =====
        public Consumable Consumable
        {
            get => _consumable;
            set
            {
                _consumable = value;
                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get => _consumable?.Name;
            set
            {
                if (_consumable != null)
                {
                    _consumable.Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int ConsumableTypeId
        {
            get => _consumable?.ConsumableTypeId ?? 0;
            set
            {
                if (_consumable != null)
                {
                    _consumable.ConsumableTypeId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Consumable GetConsumable() => _consumable;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_consumable.Name, "Название расходника"))
                return false;

            if (_consumable.Name?.Length > 100)
                AddValidationError("Название расходника не должно превышать 100 символов");

            if (_consumable.ConsumableTypeId <= 0)
                AddValidationError("Выберите тип расходника");

            return !HasErrors;
        }
    }
}
