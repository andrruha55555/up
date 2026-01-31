using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdminUP.Models;
using AdminUP.Services;

namespace AdminUP.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для EquipmentEditControl.xaml
    /// </summary>
    public partial class EquipmentEditControl : UserControl
    {
        private Equipment _equipment;
        private ApiService _apiService;

        public ObservableCollection<Classroom> AvailableClassrooms { get; set; }
        public ObservableCollection<User> AvailableUsers { get; set; }
        public ObservableCollection<Direction> AvailableDirections { get; set; }
        public ObservableCollection<Status> AvailableStatuses { get; set; }
        public ObservableCollection<ModelEntity> AvailableModels { get; set; }
        public EquipmentEditControl(Equipment equipment = null)
        {
            InitializeComponent();

            _equipment = equipment ?? new Equipment();
            _apiService = new ApiService();

            DataContext = this;
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            AvailableClassrooms = new ObservableCollection<Classroom>(
                await _apiService.GetListAsync<Classroom>("ClassroomsController"));
            AvailableUsers = new ObservableCollection<User>(
                await _apiService.GetListAsync<User>("UsersController"));
            AvailableDirections = new ObservableCollection<Direction>(
                await _apiService.GetListAsync<Direction>("DirectionsController"));
            AvailableStatuses = new ObservableCollection<Status>(
                await _apiService.GetListAsync<Status>("StatusesController"));
            AvailableModels = new ObservableCollection<ModelEntity>(
                await _apiService.GetListAsync<ModelEntity>("ModelsController"));

            OnPropertyChanged(nameof(AvailableClassrooms));
            OnPropertyChanged(nameof(AvailableUsers));
            OnPropertyChanged(nameof(AvailableDirections));
            OnPropertyChanged(nameof(AvailableStatuses));
            OnPropertyChanged(nameof(AvailableModels));
        }

        public Equipment GetEquipment()
        {
            return _equipment;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_equipment.name) ||
                string.IsNullOrWhiteSpace(_equipment.inventory_number))
            {
                MessageBox.Show("Заполните обязательные поля: Название и Инвентарный номер",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = false;
            Window.GetWindow(this).Close();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
