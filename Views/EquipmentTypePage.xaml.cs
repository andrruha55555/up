using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class EquipmentTypePage : Page
    {
        private EquipmentTypePageViewModel _viewModel;

        public EquipmentTypePage()
        {
            InitializeComponent();

            _viewModel = new EquipmentTypePageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadEquipmentTypesAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newType = new EquipmentType();
            ShowEditDialog(newType, "Добавление типа оборудования");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEquipmentType != null)
            {
                ShowEditDialog(_viewModel.SelectedEquipmentType, "Редактирование типа оборудования");
            }
            else
            {
                MessageBox.Show("Выберите тип оборудования для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEquipmentType != null)
            {
                await _viewModel.DeleteEquipmentTypeAsync(_viewModel.SelectedEquipmentType.Id);
            }
            else
            {
                MessageBox.Show("Выберите тип оборудования для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterEquipmentTypes();
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

        private async void ShowEditDialog(EquipmentType equipmentType, string title)
        {
            var editDialog = new EditDialog(equipmentType, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedType = editDialog.GetEditedItem() as EquipmentType;
                if (editedType != null)
                {
                    if (editedType.Id == 0)
                        await _viewModel.AddEquipmentTypeAsync(editedType);
                    else
                        await _viewModel.UpdateEquipmentTypeAsync(editedType.Id, editedType);
                }
            }
        }
    }
}