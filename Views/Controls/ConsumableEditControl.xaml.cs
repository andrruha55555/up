using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class ConsumableEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Consumable _consumable;

        public ObservableCollection<ConsumableType> AvailableConsumableTypes { get; private set; } = new();
        public ObservableCollection<User> AvailableUsers { get; private set; } = new();
        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ConsumableEditControl(Consumable? consumable = null)
        {
            InitializeComponent();

            _consumable = consumable ?? new Consumable();

            DataContext = this;

            _ = LoadTypesAsync();
            _ = LoadUsersAsync();
        }

        // ===== Helpers =====
        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearValidationErrors()
        {
            ValidationErrors.Clear();
            RaisePropertyChanged(nameof(HasErrors));
            RaisePropertyChanged(nameof(ValidationErrors));
        }

        private void AddValidationError(string message)
        {
            ValidationErrors.Add(message);
            RaisePropertyChanged(nameof(HasErrors));
            RaisePropertyChanged(nameof(ValidationErrors));
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

        // ===== Data load =====
        private async Task LoadTypesAsync()
        {
            var types = await App.ApiService.GetListAsync<ConsumableType>("ConsumableTypesController");
            if (types != null)
            {
                AvailableConsumableTypes.Clear();
                foreach (var t in types)
                    AvailableConsumableTypes.Add(t);

                RaisePropertyChanged(nameof(AvailableConsumableTypes));
            }
        }

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            var users = await App.ApiService.GetListAsync<User>("UsersController");
            if (users != null)
            {
                AvailableUsers.Clear();
                AvailableUsers.Add(new User { id = 0, last_name = "—", first_name = "", middle_name = "" });
                foreach (var u in users) AvailableUsers.Add(u);
                RaisePropertyChanged(nameof(AvailableUsers));
            }
        }
        // ===== Bindings =====
        public Consumable Consumable
        {
            get => _consumable;
            set
            {
                _consumable = value;
                RaisePropertyChanged();
            }
        }

        public string? ConsumableName
        {
            get => _consumable?.name;
            set
            {
                if (_consumable != null)
                {
                    _consumable.name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ImagePath
        {
            get => _consumable?.image_path ?? "";
            set
            {
                if (_consumable != null)
                {
                    _consumable.image_path = value;
                    // ✅ CS7036: было OnPropertyChanged() — конфликт с FrameworkElement.OnPropertyChanged(DependencyPropertyChangedEventArgs)
                    // Заменено на RaisePropertyChanged() — наш собственный метод
                    RaisePropertyChanged();
                }
            }
        }

        public int ConsumableTypeId
        {
            get => _consumable?.consumable_type_id ?? 0;
            set
            {
                if (_consumable != null)
                {
                    _consumable.consumable_type_id = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int Quantity
        {
            get => _consumable?.quantity ?? 0;
            set { if (_consumable != null) { _consumable.quantity = value; RaisePropertyChanged(); } }
        }
        public string? Description
        {
            get => _consumable?.description;
            set { if (_consumable != null) { _consumable.description = value; RaisePropertyChanged(); } }
        }
        public DateTime ArrivalDate
        {
            get => _consumable?.arrival_date ?? DateTime.Now;
            set { if (_consumable != null) { _consumable.arrival_date = value; RaisePropertyChanged(); } }
        }
        public int? ResponsibleUserId
        {
            get => _consumable?.responsible_user_id;
            set { if (_consumable != null) { _consumable.responsible_user_id = (value == 0) ? null : value; RaisePropertyChanged(); } }
        }
        public Consumable GetConsumable() => _consumable;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (!ValidateRequiredField(_consumable.name, "Название расходника"))
                return false;

            if (_consumable.name?.Length > 100)
                AddValidationError("Название расходника не должно превышать 100 символов");

            if (_consumable.quantity < 0)
                AddValidationError("Количество не может быть отрицательным.");
            if (_consumable.consumable_type_id <= 0)
                AddValidationError("Выберите тип расходника");

            return !HasErrors;
        }

        private void PickPhotoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var path = App.PhotoService.PickAndSave("cons");
            if (path != null)
            {
                ConsImagePathBox.Text = path;
                ImagePath = path;
            }
        }
    }
}