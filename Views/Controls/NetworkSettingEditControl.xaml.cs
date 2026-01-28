using AdminUP.Helpers;
using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class NetworkSettingEditControl : BaseEditControl
    {
        private NetworkSetting _networkSetting;
        private ApiService _apiService;

        public ObservableCollection<Equipment> AvailableEquipment { get; set; }

        public NetworkSettingEditControl(NetworkSetting networkSetting = null)
        {
            InitializeComponent();

            _networkSetting = networkSetting ?? new NetworkSetting
            {
                CreatedAt = DateTime.Now
            };

            _apiService = new ApiService();
            AvailableEquipment = new ObservableCollection<Equipment>();

            DataContext = this;
            LoadEquipmentAsync();
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

        public int EquipmentId
        {
            get => _networkSetting?.EquipmentId ?? 0;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.EquipmentId = value;
                    RaisePropertyChanged(nameof(EquipmentId));
                }
            }
        }

        public string IpAddress
        {
            get => _networkSetting?.IpAddress;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.IpAddress = value;
                    RaisePropertyChanged(nameof(IpAddress));
                }
            }
        }

        public string SubnetMask
        {
            get => _networkSetting?.SubnetMask;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.SubnetMask = value;
                    RaisePropertyChanged(nameof(SubnetMask));
                }
            }
        }

        public string Gateway
        {
            get => _networkSetting?.Gateway;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.Gateway = value;
                    RaisePropertyChanged(nameof(Gateway));
                }
            }
        }

        public string Dns1
        {
            get => _networkSetting?.Dns1;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.Dns1 = value;
                    RaisePropertyChanged(nameof(Dns1));
                }
            }
        }

        public string Dns2
        {
            get => _networkSetting?.Dns2;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.Dns2 = value;
                    RaisePropertyChanged(nameof(Dns2));
                }
            }
        }

        public DateTime CreatedAt
        {
            get => _networkSetting?.CreatedAt ?? DateTime.Now;
            set
            {
                if (_networkSetting != null)
                {
                    _networkSetting.CreatedAt = value;
                    RaisePropertyChanged(nameof(CreatedAt));
                }
            }
        }

        public NetworkSetting GetNetworkSetting()
        {
            return _networkSetting;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (_networkSetting.EquipmentId <= 0)
                AddValidationError("Выберите оборудование");

            if (!ValidateRequiredField(_networkSetting.IpAddress, "IP адрес"))
                return false;

            if (!ValidationHelper.ValidateIpAddress(_networkSetting.IpAddress))
                AddValidationError("Некорректный IP адрес");

            if (!ValidateRequiredField(_networkSetting.SubnetMask, "Маска подсети"))
                return false;

            if (!ValidationHelper.ValidateIpAddress(_networkSetting.SubnetMask))
                AddValidationError("Некорректная маска подсети");

            if (!ValidateRequiredField(_networkSetting.Gateway, "Шлюз"))
                return false;

            if (!ValidationHelper.ValidateIpAddress(_networkSetting.Gateway))
                AddValidationError("Некорректный адрес шлюза");

            if (!string.IsNullOrWhiteSpace(_networkSetting.Dns1) &&
                !ValidationHelper.ValidateIpAddress(_networkSetting.Dns1))
                AddValidationError("Некорректный DNS1");

            if (!string.IsNullOrWhiteSpace(_networkSetting.Dns2) &&
                !ValidationHelper.ValidateIpAddress(_networkSetting.Dns2))
                AddValidationError("Некорректный DNS2");

            return !HasErrors;
        }
    }
}