using AdminUP.Models;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class ConsumableTypeEditControl : BaseEditControl
    {
        private ConsumableType _consumableType;

        public ConsumableTypeEditControl(ConsumableType consumableType = null)
        {
            InitializeComponent();
            _consumableType = consumableType ?? new ConsumableType();
            DataContext = this;
        }

        public string Name
        {
            get => _consumableType?.Name;
            set
            {
                if (_consumableType != null)
                {
                    _consumableType.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public ConsumableType GetConsumableType()
        {
            return _consumableType;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_consumableType.Name, "Тип расходника"))
                return false;

            if (_consumableType.Name?.Length > 100)
                AddValidationError("Тип расходника не должен превышать 100 символов");

            return !HasErrors;
        }
    }
}