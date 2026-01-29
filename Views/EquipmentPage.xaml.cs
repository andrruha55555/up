using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class EquipmentPage : Page
    {
        private EquipmentPageViewModel _viewModel;

        public EquipmentPage()
        {
            InitializeComponent();

            _viewModel = new EquipmentPageViewModel(App.ApiService, App.CacheService);
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
            if (_viewModel.SelectedEquipment != null)
            {
                ShowEditDialog(_viewModel.SelectedEquipment, "Редактирование оборудования");
            }
            else
            {
                MessageBox.Show("Выберите оборудование для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEquipment != null)
            {
                await _viewModel.DeleteEquipmentAsync(_viewModel.SelectedEquipment.Id);
            }
            else
            {
                MessageBox.Show("Выберите оборудование для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
            var editDialog = new EditDialog(equipment, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedEquipment = editDialog.GetEditedItem() as Equipment;
                if (editedEquipment != null)
                {
                    if (editedEquipment.Id == 0)
                        await _viewModel.AddEquipmentAsync(editedEquipment);
                    else
                        await _viewModel.UpdateEquipmentAsync(editedEquipment.Id, editedEquipment);
                }
            }
        }
    }
}