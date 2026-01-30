using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
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
                _viewModel.SelectedEquipmentId = selectedId;
        }

        private void SoftwareComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SoftwareComboBox.SelectedValue is int selectedId)
                _viewModel.SelectedSoftwareId = selectedId;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(EquipmentSoftware item, string title)
        {
            // ВАЖНО: у связи нет Id, ключ составной (EquipmentId + SoftwareId)
            bool isNew = item.EquipmentId == 0 && item.SoftwareId == 0;

            var control = new EquipmentSoftwareEditControl(item);   // ✅ UserControl
            var editDialog = new EditDialog(control, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var edited = control.GetEquipmentSoftware();

                if (isNew)
                    await _viewModel.AddEquipmentSoftwareAsync(edited);
                else
                    await _viewModel.UpdateEquipmentSoftwareAsync(edited);
            }
        }
    }
}
