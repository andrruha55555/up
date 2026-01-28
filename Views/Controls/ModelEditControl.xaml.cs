using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class ModelEditControl : BaseEditControl
    {
        private Model _model;
        private ApiService _apiService;

        public ObservableCollection<EquipmentType> AvailableEquipmentTypes { get; set; }

        public ModelEditControl(Model model = null)
        {
            InitializeComponent();

            _model = model ?? new Model();
            _apiService = new ApiService();
            AvailableEquipmentTypes = new ObservableCollection<EquipmentType>();

            DataContext = this;
            LoadEquipmentTypesAsync();
        }

        private async Task LoadEquipmentTypesAsync()
        {
            var types = await _apiService.GetListAsync<EquipmentType>("EquipmentTypesController");
            if (types != null)
            {
                AvailableEquipmentTypes.Clear();
                foreach (var type in types)
                {
                    AvailableEquipmentTypes.Add(type);
                }
                RaisePropertyChanged(nameof(AvailableEquipmentTypes));
            }
        }

        public string Name
        {
            get => _model?.Name;
            set
            {
                if (_model != null)
                {
                    _model.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public int EquipmentTypeId
        {
            get => _model?.EquipmentTypeId ?? 0;
            set
            {
                if (_model != null)
                {
                    _model.EquipmentTypeId = value;
                    RaisePropertyChanged(nameof(EquipmentTypeId));
                }
            }
        }

        public Model GetModel()
        {
            return _model;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_model.Name, "Название модели"))
                return false;

            if (_model.Name?.Length > 100)
                AddValidationError("Название модели не должно превышать 100 символов");

            if (_model.EquipmentTypeId <= 0)
                AddValidationError("Выберите тип оборудования");

            return !HasErrors;
        }
    }
}