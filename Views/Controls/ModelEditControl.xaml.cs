using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class ModelEditControl : UserControl, INotifyPropertyChanged
    {
        private readonly ModelEntity _model;
        private readonly ApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ObservableCollection<EquipmentType> AvailableEquipmentTypes { get; } = new();

        public ModelEditControl(ModelEntity model = null)
        {
            InitializeComponent();

            _model = model ?? new ModelEntity();
            _apiService = new ApiService();

            DataContext = this;

            _ = LoadEquipmentTypesAsync();
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

        private async Task LoadEquipmentTypesAsync()
        {
            var types = await _apiService.GetListAsync<EquipmentType>("EquipmentTypesController");
            if (types != null)
            {
                AvailableEquipmentTypes.Clear();
                foreach (var type in types)
                    AvailableEquipmentTypes.Add(type);

                RaisePropertyChanged(nameof(AvailableEquipmentTypes));
            }
        }

        public string ModelName
        {
            get => _model?.name;
            set
            {
                if (_model != null)
                {
                    _model.name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public int EquipmentTypeId
        {
            get => _model?.equipment_type_id ?? 0;
            set
            {
                if (_model != null)
                {
                    _model.equipment_type_id = value;
                    RaisePropertyChanged(nameof(EquipmentTypeId));
                }
            }
        }
        public ModelEntity GetModel() => _model;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_model?.name, "Название модели"))
                return false;

            if (_model.name?.Length > 100)
                AddValidationError("Название модели не должно превышать 100 символов");

            if ((_model?.equipment_type_id ?? 0) <= 0)
                AddValidationError("Выберите тип оборудования");

            return !HasErrors;
        }
    }
}
