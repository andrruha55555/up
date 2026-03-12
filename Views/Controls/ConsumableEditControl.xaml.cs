using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdminUP.Views.Controls
{
    /// <summary>
    /// Контрол редактирования расходного материала.
    /// п. 1.11 ТЗ: все поля включая описание, дату (ДД.ММ.ГГГГ), количество (только цифры),
    /// ответственных, привязку к оборудованию и фотографию.
    /// </summary>
    public partial class ConsumableEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Consumable _consumable;
        private readonly ApiService _apiService;

        /// <summary>Доступные типы расходников</summary>
        public ObservableCollection<ConsumableType> AvailableConsumableTypes { get; private set; } = new();

        /// <summary>Доступные пользователи (ответственные)</summary>
        public ObservableCollection<User> AvailableUsers { get; private set; } = new();

        /// <summary>Доступное оборудование (для привязки)</summary>
        public ObservableCollection<Equipment> AvailableEquipment { get; private set; } = new();

        /// <summary>Ошибки валидации для отображения в UI</summary>
        public ObservableCollection<string> ValidationErrors { get; } = new();

        /// <summary>True если есть ошибки валидации</summary>
        public bool HasErrors => ValidationErrors.Count > 0;

        public ConsumableEditControl(Consumable? consumable = null)
        {
            InitializeComponent();

            _consumable = consumable ?? new Consumable
            {
                arrival_date = DateTime.Today,
                quantity = 0
            };

            _apiService = new ApiService();
            DataContext = this;

            _ = LoadDataAsync();
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearValidationErrors()
        {
            ValidationErrors.Clear();
            RaisePropertyChanged(nameof(HasErrors));
        }

        private void AddValidationError(string message)
        {
            ValidationErrors.Add(message);
            RaisePropertyChanged(nameof(HasErrors));
        }

        // ─── Валидация ────────────────────────────────────────────────────────

        /// <summary>
        /// Валидация всех полей расходника (п. 1.11 ТЗ).
        /// </summary>
        public bool Validate() => ValidateData();

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (string.IsNullOrWhiteSpace(_consumable.name))
                AddValidationError("Название расходника обязательно");

            if (_consumable.consumable_type_id <= 0)
                AddValidationError("Выберите тип расходного материала");

            // п. 1.11 ТЗ: "в поле количество можно указать только цифры"
            if (_consumable.quantity < 0)
                AddValidationError("Количество не может быть отрицательным");

            // п. 1.11 ТЗ: "дата поступления имеет формат ДД.ММ.ГГГГ"
            if (_consumable.arrival_date > DateTime.Now)
                AddValidationError("Дата поступления не может быть в будущем");

            return !HasErrors;
        }

        // ─── Загрузка данных ─────────────────────────────────────────────────

        private async Task LoadDataAsync()
        {
            try
            {
                var types = await _apiService.GetListAsync<ConsumableType>("ConsumableTypesController");
                AvailableConsumableTypes.Clear();
                if (types != null)
                    foreach (var t in types) AvailableConsumableTypes.Add(t);
                RaisePropertyChanged(nameof(AvailableConsumableTypes));

                var users = await _apiService.GetListAsync<User>("UsersController");
                AvailableUsers.Clear();
                AvailableUsers.Add(new User { id = 0, last_name = "—", first_name = "Не назначен" });
                if (users != null) foreach (var u in users) AvailableUsers.Add(u);
                RaisePropertyChanged(nameof(AvailableUsers));

                var equipment = await _apiService.GetListAsync<Equipment>("EquipmentController");
                AvailableEquipment.Clear();
                AvailableEquipment.Add(new Equipment { id = 0, name = "— Не привязано —" });
                if (equipment != null) foreach (var eq in equipment) AvailableEquipment.Add(eq);
                RaisePropertyChanged(nameof(AvailableEquipment));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных:\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                App.LogError(ex, "ConsumableEditControl.LoadDataAsync");
            }
        }

        // ─── Свойства (биндинги) ──────────────────────────────────────────────

        /// <summary>Возвращает итоговый объект расходника</summary>
        public Consumable GetConsumable() => _consumable;

        /// <summary>Название расходника</summary>
        public string? ConsumableName
        {
            get => _consumable?.name;
            set { if (_consumable != null) { _consumable.name = value; RaisePropertyChanged(); } }
        }

        /// <summary>Описание расходника</summary>
        public string? Description
        {
            get => _consumable?.description;
            set { if (_consumable != null) { _consumable.description = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Дата поступления в виде строки ДД.ММ.ГГГГ (п. 1.11 ТЗ).
        /// При изменении парсится и сохраняется в модель.
        /// </summary>
        public string ArrivalDateText
        {
            get => _consumable?.arrival_date.ToString("dd.MM.yyyy") ?? "";
            set
            {
                if (_consumable == null) return;

                if (DateTime.TryParseExact(value, "dd.MM.yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var dt))
                {
                    _consumable.arrival_date = dt;
                }
                else if (DateTime.TryParse(value, out var dt2))
                {
                    _consumable.arrival_date = dt2;
                }

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Количество в виде строки (п. 1.11 ТЗ: только цифры).
        /// </summary>
        public string QuantityText
        {
            get => _consumable?.quantity.ToString() ?? "0";
            set
            {
                if (_consumable == null) return;
                if (int.TryParse(value, out var qty) && qty >= 0)
                    _consumable.quantity = qty;
                RaisePropertyChanged();
            }
        }

        /// <summary>ID типа расходника</summary>
        public int ConsumableTypeId
        {
            get => _consumable?.consumable_type_id ?? 0;
            set { if (_consumable != null) { _consumable.consumable_type_id = value; RaisePropertyChanged(); } }
        }

        /// <summary>ID ответственного пользователя</summary>
        public int? ResponsibleUserId
        {
            get => _consumable?.responsible_user_id;
            set { if (_consumable != null) { _consumable.responsible_user_id = (value == 0) ? null : value; RaisePropertyChanged(); } }
        }

        /// <summary>ID временно ответственного пользователя</summary>
        public int? TempResponsibleUserId
        {
            get => _consumable?.temp_responsible_user_id;
            set { if (_consumable != null) { _consumable.temp_responsible_user_id = (value == 0) ? null : value; RaisePropertyChanged(); } }
        }

        /// <summary>ID оборудования к которому привязан расходник (п. 1.11 ТЗ)</summary>
        public int? AttachedEquipmentId
        {
            get => _consumable?.attached_to_equipment_id;
            set { if (_consumable != null) { _consumable.attached_to_equipment_id = (value == 0) ? null : value; RaisePropertyChanged(); } }
        }

        /// <summary>Путь к фотографии расходника</summary>
        public string ImagePath
        {
            get => _consumable?.image_path ?? "";
            set { if (_consumable != null) { _consumable.image_path = value; RaisePropertyChanged(); } }
        }

        // ─── Ограничение ввода ────────────────────────────────────────────────

        /// <summary>
        /// Дата — разрешены только цифры и точки в формате ДД.ММ.ГГГГ.
        /// </summary>
        private void DateBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d\.]");
        }

        /// <summary>
        /// Количество — только цифры (п. 1.11 ТЗ).
        /// </summary>
        private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        /// <summary>Защита от вставки нецифрового текста в поле количества.</summary>
        private void QuantityBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, @"^\d+$"))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        // ─── Кнопка фото ─────────────────────────────────────────────────────

        private void PickPhotoButton_Click(object sender, RoutedEventArgs e)
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
