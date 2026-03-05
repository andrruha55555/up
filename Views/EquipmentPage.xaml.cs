using AdminUP.Models;
using AdminUP.Services;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class EquipmentPage : Page
    {
        private EquipmentPageViewModel _viewModel;
        private readonly ExportService _exportService;

        public EquipmentPage()
        {
            InitializeComponent();

            _viewModel = new EquipmentPageViewModel(App.ApiService, App.CacheService);
            _exportService = new ExportService(App.ApiService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadEquipmentAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newEquipment = new Equipment();
            ShowEditDialog(newEquipment, "Добавление оборудования");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedEquipment;
            if (row != null)
                ShowEditDialog(row.Equipment, "Редактирование оборудования");
            else
                MessageBox.Show("Выберите оборудование для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedEquipment;
            if (row != null)
                await _viewModel.DeleteEquipmentAsync(row.Equipment.id);
            else
                MessageBox.Show("Выберите оборудование для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            await _exportService.ExportEquipmentReportAsync();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterEquipment();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(Equipment equipment, string title)
        {
            var control = new EquipmentEditControl(equipment);
            var editDialog = new EditDialog(control, title) { Owner = Window.GetWindow(this) };

            if (editDialog.ShowDialog() == true)
            {
                var editedEquipment = control.GetEquipment();
                if (editedEquipment != null)
                {
                    if (editedEquipment.id == 0)
                        await _viewModel.AddEquipmentAsync(editedEquipment);
                    else
                        await _viewModel.UpdateEquipmentAsync(editedEquipment.id, editedEquipment);
                }
            }
        }
    }
}
