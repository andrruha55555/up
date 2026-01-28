using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentHistoryEditControl : BaseEditControl
    {
        private EquipmentHistory _equipmentHistory;
        private ApiService _apiService;

        public ObservableCollection<Equipment> AvailableEquipment { get; set; }
        public ObservableCollection<Classroom> AvailableClassrooms { get; set; }
        public ObservableCollection<User> AvailableUsers { get; set; }

        public EquipmentHistoryEditControl(EquipmentHistory equipmentHistory = null)
        {
            InitializeComponent();

            _equipmentHistory = equipmentHistory ?? new EquipmentHistory
            {
                ChangedAt = DateTime.Now
            };

            _apiService = new ApiService();
            AvailableEquipment = new ObservableCollection<Equipment>();
            AvailableClassrooms = new ObservableCollection<Classroom>();
            AvailableUsers = new ObservableCollection<User>();

            DataContext = this;
            LoadDataAsync();
        }

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
                {
                    AvailableEquipment.Add(item);
                }
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
                {
                    AvailableClassrooms.Add(classroom);
                }
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
                {
                    AvailableUsers.Add(user);
                }
                RaisePropertyChanged(nameof(AvailableUsers));
            }
        }

        public int EquipmentId
        {
            get => _equipmentHistory?.EquipmentId ?? 0;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.EquipmentId = value;
                    RaisePropertyChanged(nameof(EquipmentId));
                }
            }
        }

        public int? ClassroomId
        {
            get => _equipmentHistory?.ClassroomId;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.ClassroomId = value;
                    RaisePropertyChanged(nameof(ClassroomId));
                }
            }
        }

        public int? ResponsibleUserId
        {
            get => _equipmentHistory?.ResponsibleUserId;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.ResponsibleUserId = value;
                    RaisePropertyChanged(nameof(ResponsibleUserId));
                }
            }
        }

        public string Comment
        {
            get => _equipmentHistory?.Comment;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.Comment = value;
                    RaisePropertyChanged(nameof(Comment));
                }
            }
        }

        public DateTime ChangedAt
        {
            get => _equipmentHistory?.ChangedAt ?? DateTime.Now;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.ChangedAt = value;
                    RaisePropertyChanged(nameof(ChangedAt));
                }
            }
        }

        public int? ChangedByUserId
        {
            get => _equipmentHistory?.ChangedByUserId;
            set
            {
                if (_equipmentHistory != null)
                {
                    _equipmentHistory.ChangedByUserId = value;
                    RaisePropertyChanged(nameof(ChangedByUserId));
                }
            }
        }

        public EquipmentHistory GetEquipmentHistory()
        {
            return _equipmentHistory;
        }

        protected override bool ValidateData()
        {
            ClearValidationErrors();

            if (_equipmentHistory.EquipmentId <= 0)
                AddValidationError("Выберите оборудование");

            return !HasErrors;
        }
    }
}
