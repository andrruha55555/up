using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentHistoryEditControl : UserControl, INotifyPropertyChanged
    {
        private EquipmentHistory _equipmentHistory;
        private readonly ApiService _apiService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> ValidationErrors { get; } = new();
        public bool HasErrors => ValidationErrors.Count > 0;

        public ObservableCollection<Equipment> AvailableEquipment { get; set; }
        public ObservableCollection<Classroom> AvailableClassrooms { get; set; }
        public ObservableCollection<User> AvailableUsers { get; set; }

        public EquipmentHistoryEditControl(EquipmentHistory equipmentHistory = null)
        {
            InitializeComponent();

            _equipmentHistory = equipmentHistory ?? new EquipmentHistory
            {
                changed_at = DateTime.Now
            };

            _apiService = new ApiService();
            AvailableEquipment = new ObservableCollection<Equipment>();
            AvailableClassrooms = new ObservableCollection<Classroom>();
            AvailableUsers = new ObservableCollection<User>();

            DataContext = this;
            _ = LoadDataAsync();
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearValidationErrors()
        {
            ValidationErrors.Clear();
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(HasErrors));
        }

        private void AddValidationError(string message)
        {
            ValidationErrors.Add(message);
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(HasErrors));
        }

        public bool Validate() => ValidateData();

        private async Task LoadDataAsync()
        {
            await Task.WhenAll(
                LoadEquipmentAsync(),
                LoadClassroomsAsync(),
                LoadUsersAsync()
            );
        }

        private async Task LoadEquipmentAsync()
        {
            var equipment = await _apiService.GetListAsync<Equipment>("EquipmentController");
            if (equipment != null)
            {
                AvailableEquipment.Clear();
                foreach (var item in equipment)
                    AvailableEquipment.Add(item);

                RaisePropertyChanged(nameof(AvailableEquipment));
            }
        }

        private async Task LoadClassroomsAsync()
        {
            var classrooms = await _apiService.GetListAsync<Classroom>("ClassroomsController");
            if (classrooms != null)
            {
                AvailableClassrooms.Clear();
                foreach (var classroom in classrooms)
                    AvailableClassrooms.Add(classroom);

                RaisePropertyChanged(nameof(AvailableClassrooms));
            }
        }

        private async Task LoadUsersAsync()
        {
            var users = await _apiService.GetListAsync<User>("UsersController");
            if (users != null)
            {
                AvailableUsers.Clear();
                foreach (var user in users)
                    AvailableUsers.Add(user);

                RaisePropertyChanged(nameof(AvailableUsers));
            }
        }

        public int EquipmentId
        {
            get => _equipmentHistory?.equipment_id ?? 0;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.equipment_id = value;
                    RaisePropertyChanged(nameof(EquipmentId));
                }
            }
        }

        public int? ClassroomId
        {
            get => _equipmentHistory?.classroom_id;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.classroom_id = value;
                    RaisePropertyChanged(nameof(ClassroomId));
                }
            }
        }

        public int? ResponsibleUserId
        {
            get => _equipmentHistory?.responsible_user_id;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.responsible_user_id = value;
                    RaisePropertyChanged(nameof(ResponsibleUserId));
                }
            }
        }

        public string Comment
        {
            get => _equipmentHistory?.comment;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.comment = value;
                    RaisePropertyChanged(nameof(Comment));
                }
            }
        }

        public DateTime ChangedAt
        {
            get => _equipmentHistory?.changed_at ?? DateTime.Now;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.changed_at = value;
                    RaisePropertyChanged(nameof(ChangedAt));
                }
            }
        }

        public int? ChangedByUserId
        {
            get => _equipmentHistory?.changed_by_user_id;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.changed_by_user_id = value;
                    RaisePropertyChanged(nameof(ChangedByUserId));
                }
            }
        }

        public EquipmentHistory GetEquipmentHistory() => _equipmentHistory;

        private bool ValidateData()
        {
            ClearValidationErrors();

            if (_equipmentHistory.equipment_id <= 0)
                AddValidationError("Выберите оборудование");

            return !HasErrors;
        }
    }
}
