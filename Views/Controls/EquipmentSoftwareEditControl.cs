using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentSoftwareEditControl : UserControl, INotifyPropertyChanged
    {
        private readonly EquipmentSoftware _equipmentSoftware;
        private readonly ApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ObservableCollection<Equipment> AvailableEquipment { get; set; }
        public ObservableCollection<Software> AvailableSoftware { get; set; }

        public EquipmentSoftwareEditControl(EquipmentSoftware equipmentSoftware = null)
        {
            // ВАЖНО: если у тебя НЕТ XAML для этого контрола, то строки InitializeComponent() быть не должно.
            // Если XAML ЕСТЬ (EquipmentSoftwareEditControl.xaml), то оставь InitializeComponent();
            // Иначе закомментируй/удали.

            // InitializeComponent();

            _equipmentSoftware = equipmentSoftware ?? new EquipmentSoftware();
            _apiService = new ApiService();
            AvailableEquipment = new ObservableCollection<Equipment>();
            AvailableSoftware = new ObservableCollection<Software>();

            DataContext = this;
            _ = LoadDataAsync();
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

        public bool Validate() => ValidateData();

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
                    AvailableEquipment.Add(item);

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
                    AvailableSoftware.Add(item);

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

        public EquipmentSoftware GetEquipmentSoftware() => _equipmentSoftware;

        private bool ValidateData()
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
