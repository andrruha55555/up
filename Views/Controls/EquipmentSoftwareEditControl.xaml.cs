using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentSoftwareEditControl : BaseEditControl
    {
        private EquipmentSoftware _equipmentSoftware;
        private ApiService _apiService;

        public ObservableCollection<Equipment> AvailableEquipment { get; set; }
        public ObservableCollection<Software> AvailableSoftware { get; set; }

        public EquipmentSoftwareEditControl(EquipmentSoftware equipmentSoftware = null)
        {
            InitializeComponent();

            _equipmentSoftware = equipmentSoftware ?? new EquipmentSoftware();
            _apiService = new ApiService();
            AvailableEquipment = new ObservableCollection<Equipment>();
            AvailableSoftware = new ObservableCollection<Software>();

            DataContext = this;
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await Task.WhenAll(
                LoadEquipmentAsync(),
                LoadSoftwareAsync()
            );
        }

        private async Task LoadEquipmentAsync()
        {
            var equipment = await _apiService.GetListAsync<Equipment>("EquipmentController");
            if (equipment != null)
            {
                AvailableEquipment.Clear();
                foreach (var item in equipment)
                {
                    AvailableEquipment.Add(item);
                }
                RaisePropertyChanged(nameof(AvailableEquipment));
            }
        }

        private async Task LoadSoftwareAsync()
        {
            var software = await _apiService.GetListAsync<Software>("SoftwareController");
            if (software != null)
            {
                AvailableSoftware.Clear();
                foreach (var item in software)
                {
                    AvailableSoftware.Add(item);
                }
                RaisePropertyChanged(nameof(AvailableSoftware));
            }
        }

        public int EquipmentId
        {
            get => _equipmentSoftware?.EquipmentId ?? 0;
            set
            {
                if (_equipmentSoftware != null)
                {
                    _equipmentSoftware.EquipmentId = value;
                    RaisePropertyChanged(nameof(EquipmentId));
                }
            }
        }

        public int SoftwareId
        {
            get => _equipmentSoftware?.SoftwareId ?? 0;
            set
            {
                if (_equipmentSoftware != null)
                {
                    _equipmentSoftware.SoftwareId = value;
                    RaisePropertyChanged(nameof(SoftwareId));
                }
            }
        }

        public EquipmentSoftware GetEquipmentSoftware()
        {
            return _equipmentSoftware;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (_equipmentSoftware.EquipmentId <= 0)
                AddValidationError("Выберите оборудование");

            if (_equipmentSoftware.SoftwareId <= 0)
                AddValidationError("Выберите программное обеспечение");

            return !HasErrors;
        }
    }
}