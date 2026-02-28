using AdminUP.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Equipment _equipment;
        private bool _loaded;

        public ObservableCollection<Classroom> AvailableClassrooms { get; } = new();
        public ObservableCollection<User> AvailableUsers { get; } = new();
        public ObservableCollection<Direction> AvailableDirections { get; } = new();
        public ObservableCollection<Status> AvailableStatuses { get; } = new();
        public ObservableCollection<ModelEntity> AvailableModels { get; } = new();

        public EquipmentEditControl(Equipment equipment = null)
        {
            InitializeComponent();

            _equipment = equipment ?? new Equipment();

            DataContext = this;

            _ = LoadDataAsync();
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // под EditDialog
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(_equipment.name))
            {
                MessageBox.Show("Название обязательно для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_equipment.inventory_number))
            {
                MessageBox.Show("Инвентарный номер обязателен для заполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_equipment.status_id == 0)
            {
                MessageBox.Show("Статус обязателен для выбора", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public object GetEditedItem() => _equipment;
        public Equipment GetEquipment() => _equipment;

        // если в XAML есть Loaded="UserControl_Loaded" — оставь
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded) return;
            _loaded = true;

            DataContext = this;

            RaisePropertyChanged(nameof(EquipmentName));
            RaisePropertyChanged(nameof(EquipmentInventoryNumber));
            RaisePropertyChanged(nameof(ClassroomId));
            RaisePropertyChanged(nameof(ResponsibleUserId));
            RaisePropertyChanged(nameof(DirectionId));
            RaisePropertyChanged(nameof(StatusId));
            RaisePropertyChanged(nameof(ModelId));
            RaisePropertyChanged(nameof(Comment));
            RaisePropertyChanged(nameof(ImagePath));
            RaisePropertyChanged(nameof(Cost));
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var classrooms = await App.ApiService.GetListAsync<Classroom>("ClassroomsController");
                AvailableClassrooms.Clear();
                AvailableClassrooms.Add(new Classroom { id = 0, name = "Не выбрано", short_name = "" });
                if (classrooms != null) foreach (var c in classrooms) AvailableClassrooms.Add(c);
                RaisePropertyChanged(nameof(AvailableClassrooms));

                var users = await App.ApiService.GetListAsync<User>("UsersController");
                AvailableUsers.Clear();
                AvailableUsers.Add(new User { id = 0, last_name = "Не", first_name = "назначен", middle_name = "" });
                if (users != null) foreach (var u in users) AvailableUsers.Add(u);
                RaisePropertyChanged(nameof(AvailableUsers));

                var directions = await App.ApiService.GetListAsync<Direction>("DirectionsController");
                AvailableDirections.Clear();
                AvailableDirections.Add(new Direction { id = 0, name = "Не выбрано" });
                if (directions != null) foreach (var d in directions) AvailableDirections.Add(d);
                RaisePropertyChanged(nameof(AvailableDirections));

                var statuses = await App.ApiService.GetListAsync<Status>("StatusesController");
                AvailableStatuses.Clear();
                AvailableStatuses.Add(new Status { id = 0, name = "Не выбрано" });
                if (statuses != null) foreach (var s in statuses) AvailableStatuses.Add(s);
                RaisePropertyChanged(nameof(AvailableStatuses));

                var models = await App.ApiService.GetListAsync<ModelEntity>("ModelsController");
                AvailableModels.Clear();
                AvailableModels.Add(new ModelEntity { id = 0, name = "Не выбрано", equipment_type_id = 0 });
                if (models != null) foreach (var m in models) AvailableModels.Add(m);
                RaisePropertyChanged(nameof(AvailableModels));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списков:\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== свойства под XAML =====

        public string EquipmentName
        {
            get => _equipment?.name ?? "";
            set { _equipment.name = value; RaisePropertyChanged(); }
        }

        public string EquipmentInventoryNumber
        {
            get => _equipment?.inventory_number ?? "";
            set { _equipment.inventory_number = value; RaisePropertyChanged(); }
        }

        public int? ClassroomId
        {
            get => _equipment?.classroom_id;
            set { _equipment.classroom_id = (value == 0) ? null : value; RaisePropertyChanged(); }
        }

        public int? ResponsibleUserId
        {
            get => _equipment?.responsible_user_id;
            set { _equipment.responsible_user_id = (value == 0) ? null : value; RaisePropertyChanged(); }
        }

        public int? DirectionId
        {
            get => _equipment?.direction_id;
            set { _equipment.direction_id = (value == 0) ? null : value; RaisePropertyChanged(); }
        }

        public int StatusId
        {
            get => _equipment?.status_id ?? 0;
            set { _equipment.status_id = value; RaisePropertyChanged(); }
        }

        public int? ModelId
        {
            get => _equipment?.model_id;
            set { _equipment.model_id = (value == 0) ? null : value; RaisePropertyChanged(); }
        }

        public string Comment
        {
            get => _equipment?.comment ?? "";
            set { _equipment.comment = value; RaisePropertyChanged(); }
        }

        public string ImagePath
        {
            get => _equipment?.image_path ?? "";
            set { _equipment.image_path = value; RaisePropertyChanged(); }
        }

        public string Cost
        {
            get => _equipment?.cost?.ToString(CultureInfo.InvariantCulture) ?? "";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _equipment.cost = null;
                    RaisePropertyChanged();
                    return;
                }

                if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    _equipment.cost = d;
                    RaisePropertyChanged();
                }
            }
        }
    }
}