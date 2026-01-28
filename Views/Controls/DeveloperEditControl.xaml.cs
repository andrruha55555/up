using AdminUP.Models;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class DeveloperEditControl : BaseEditControl
    {
        private Developer _developer;

        public DeveloperEditControl(Developer developer = null)
        {
            InitializeComponent();
            _developer = developer ?? new Developer();
            DataContext = this;
        }

        public string Name
        {
            get => _developer?.Name;
            set
            {
                if (_developer != null)
                {
                    _developer.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public Developer GetDeveloper()
        {
            return _developer;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_developer.Name, "Название разработчика"))
                return false;

            if (_developer.Name?.Length > 100)
                AddValidationError("Название разработчика не должно превышать 100 символов");

            return !HasErrors;
        }
    }
}