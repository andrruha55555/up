using AdminUP.Helpers;
using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class NetworkSettingEditControl : UserControl, INotifyPropertyChanged
    {
        private readonly NetworkSetting _networkSetting;
        private readonly ApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ObservableCollection<Equipment> AvailableEquipment { get; } = new();

        public NetworkSettingEditControl(NetworkSetting networkSetting = null)
        {
            InitializeComponent();

            _networkSetting = networkSetting ?? new NetworkSetting();
            _apiService = new ApiService();

            DataContext = this;

            _ = LoadEquipmentAsync();
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

        public int EquipmentId
        {
            get => _networkSetting?.equipment_id ?? 0;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.equipment_id = value;
                    RaisePropertyChanged(nameof(EquipmentId));
                }
            }
        }

        public string IpAddress
        {
            get => _networkSetting?.ip_address;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.ip_address = value;
                    RaisePropertyChanged(nameof(IpAddress));
                }
            }
        }

        public string SubnetMask
        {
            get => _networkSetting?.subnet_mask;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.subnet_mask = value;
                    RaisePropertyChanged(nameof(SubnetMask));
                }
            }
        }

        public string Gateway
        {
            get => _networkSetting?.gateway;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.gateway = value;
                    RaisePropertyChanged(nameof(Gateway));
                }
            }
        }

        public string Dns1
        {
            get => _networkSetting?.dns1;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.dns1 = value;
                    RaisePropertyChanged(nameof(Dns1));
                }
            }
        }

        public string Dns2
        {
            get => _networkSetting?.dns2;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.dns2 = value;
                    RaisePropertyChanged(nameof(Dns2));
                }
            }
        }

        public NetworkSetting GetNetworkSetting() => _networkSetting;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if ((_networkSetting?.equipment_id ?? 0) <= 0)
                AddValidationError("Выберите оборудование");

            if (!ValidateRequiredField(_networkSetting?.ip_address, "IP адрес"))
                return false;

            if (!ValidationHelper.ValidateIpAddress(_networkSetting.ip_address))
                AddValidationError("Некорректный IP адрес");

            if (!ValidateRequiredField(_networkSetting?.subnet_mask, "Маска подсети"))
                return false;

            if (!ValidationHelper.ValidateIpAddress(_networkSetting.subnet_mask))
                AddValidationError("Некорректная маска подсети");

            if (!ValidateRequiredField(_networkSetting?.gateway, "Шлюз"))
                return false;

            if (!ValidationHelper.ValidateIpAddress(_networkSetting.gateway))
                AddValidationError("Некорректный адрес шлюза");

            if (!string.IsNullOrWhiteSpace(_networkSetting?.dns1) &&
                !ValidationHelper.ValidateIpAddress(_networkSetting.dns1))
                AddValidationError("Некорректный DNS1");

            if (!string.IsNullOrWhiteSpace(_networkSetting?.dns2) &&
                !ValidationHelper.ValidateIpAddress(_networkSetting.dns2))
                AddValidationError("Некорректный DNS2");

            return !HasErrors;
        }
    }
}
