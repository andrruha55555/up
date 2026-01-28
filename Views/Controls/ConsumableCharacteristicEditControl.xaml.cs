using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class ConsumableCharacteristicEditControl : BaseEditControl
    {
        private ConsumableCharacteristic _characteristic;
        private ApiService _apiService;

        public ObservableCollection<Consumable> AvailableConsumables { get; set; }

        public ConsumableCharacteristicEditControl(ConsumableCharacteristic characteristic = null)
        {
            InitializeComponent();

            _characteristic = characteristic ?? new ConsumableCharacteristic();
            _apiService = new ApiService();
            AvailableConsumables = new ObservableCollection<Consumable>();

            DataContext = this;
            LoadConsumablesAsync();
        }

        private async Task LoadConsumablesAsync()
        {
            var consumables = await _apiService.GetListAsync<Consumable>("ConsumablesController");
            if (consumables != null)
            {
                AvailableConsumables.Clear();
                foreach (var consumable in consumables)
                {
                    AvailableConsumables.Add(consumable);
                }
                RaisePropertyChanged(nameof(AvailableConsumables));
            }
        }

        public int ConsumableId
        {
            get => _characteristic?.ConsumableId ?? 0;
            set
            {
                if (_characteristic != null)
                {
                    _characteristic.ConsumableId = value;
                    RaisePropertyChanged(nameof(ConsumableId));
                }
            }
        }

        public string CharacteristicName
        {
            get => _characteristic?.CharacteristicName;
            set
            {
                if (_characteristic != null)
                {
                    _characteristic.CharacteristicName = value;
                    RaisePropertyChanged(nameof(CharacteristicName));
                }
            }
        }

        public string CharacteristicValue
        {
            get => _characteristic?.CharacteristicValue;
            set
            {
                if (_characteristic != null)
                {
                    _characteristic.CharacteristicValue = value;
                    RaisePropertyChanged(nameof(CharacteristicValue));
                }
            }
        }

        public ConsumableCharacteristic GetCharacteristic()
        {
            return _characteristic;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (_characteristic.ConsumableId <= 0)
                AddValidationError("Выберите расходник");

            if (!ValidateRequiredField(_characteristic.CharacteristicName, "Название характеристики"))
                return false;

            if (_characteristic.CharacteristicName?.Length > 100)
                AddValidationError("Название характеристики не должно превышать 100 символов");

            if (!ValidateRequiredField(_characteristic.CharacteristicValue, "Значение характеристики"))
                return false;

            return !HasErrors;
        }
    }
}