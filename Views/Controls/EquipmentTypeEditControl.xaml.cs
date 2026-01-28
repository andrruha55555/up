using AdminUP.Models;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentTypeEditControl : BaseEditControl
    {
        private EquipmentType _equipmentType;

        public EquipmentTypeEditControl(EquipmentType equipmentType = null)
        {
            InitializeComponent();
            _equipmentType = equipmentType ?? new EquipmentType();
            DataContext = this;
        }

        public string Name
        {
            get => _equipmentType?.Name;
            set
            {
                if (_equipmentType != null)
                {
                    _equipmentType.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public EquipmentType GetEquipmentType()
        {
            return _equipmentType;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_equipmentType.Name, "Тип оборудования"))
                return false;

            if (_equipmentType.Name?.Length > 50)
                AddValidationError("Тип оборудования не должен превышать 50 символов");

            return !HasErrors;
        }
    }
}