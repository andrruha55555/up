using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class EquipmentSoftwarePage : Page
    {
        private EquipmentSoftwarePageViewModel _viewModel;

        public EquipmentSoftwarePage()
        {
            InitializeComponent();

            _viewModel = new EquipmentSoftwarePageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;

            // Привязка ComboBox
            EquipmentComboBox.ItemsSource = _viewModel.EquipmentList;
            SoftwareComboBox.ItemsSource = _viewModel.SoftwareList;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newEquipmentSoftware = new EquipmentSoftware();
            ShowEditDialog(newEquipmentSoftware, "Добавление связи оборудование-ПО");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEquipmentSoftware != null)
            {
                ShowEditDialog(_viewModel.SelectedEquipmentSoftware, "Редактирование связи оборудование-ПО");
            }
            else
            {
                MessageBox.Show("Выберите связь для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEquipmentSoftware != null)
            {
                await _viewModel.DeleteEquipmentSoftwareAsync(
                    _viewModel.SelectedEquipmentSoftware.EquipmentId,
                    _viewModel.SelectedEquipmentSoftware.SoftwareId);
            }
            else
            {
                MessageBox.Show("Выберите связь для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterEquipmentSoftware();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void EquipmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EquipmentComboBox.SelectedValue is int selectedId)
            {
                _viewModel.SelectedEquipmentId = selectedId;
            }
        }

        private void SoftwareComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SoftwareComboBox.SelectedValue is int selectedId)
            {
                _viewModel.SelectedSoftwareId = selectedId;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(EquipmentSoftware equipmentSoftware, string title)
        {
            var editDialog = new EditDialog(equipmentSoftware, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedEquipmentSoftware = editDialog.GetEditedItem() as EquipmentSoftware;
                if (editedEquipmentSoftware != null)
                {
                    // Для EquipmentSoftware нет ID, поэтому всегда добавляем
                    await _viewModel.AddEquipmentSoftwareAsync(editedEquipmentSoftware);
                }
            }
        }
    }
}