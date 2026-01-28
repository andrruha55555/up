using AdminUP.Models;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class DirectionEditControl : BaseEditControl
    {
        private Direction _direction;

        public DirectionEditControl(Direction direction = null)
        {
            InitializeComponent();
            _direction = direction ?? new Direction();
            DataContext = this;
        }

        public string Name
        {
            get => _direction?.Name;
            set
            {
                if (_direction != null)
                {
                    _direction.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public Direction GetDirection()
        {
            return _direction;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_direction.Name, "Название направления"))
                return false;

            if (_direction.Name?.Length > 100)
                AddValidationError("Название направления не должно превышать 100 символов");

            return !HasErrors;
        }
    }
}