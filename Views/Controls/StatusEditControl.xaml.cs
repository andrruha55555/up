using AdminUP.Models;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class StatusEditControl : BaseEditControl
    {
        private Status _status;

        public StatusEditControl(Status status = null)
        {
            InitializeComponent();
            _status = status ?? new Status();
            DataContext = this;
        }

        public string Name
        {
            get => _status?.Name;
            set
            {
                if (_status != null)
                {
                    _status.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public Status GetStatus()
        {
            return _status;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_status.Name, "Название статуса"))
                return false;

            if (_status.Name?.Length > 50)
                AddValidationError("Название статуса не должно превышать 50 символов");

            return !HasErrors;
        }
    }
}