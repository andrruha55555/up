using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class EquipmentSoftwarePage : Page
    {
        private readonly EquipmentSoftwarePageViewModel _viewModel;

        public EquipmentSoftwarePage()
        {
            InitializeComponent();
            _viewModel = new EquipmentSoftwarePageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEditDialog(new EquipmentSoftware(), "Добавление связи оборудование-ПО");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEquipmentSoftware == null)
            {
                MessageBox.Show("Выберите связь для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ShowEditDialog(_viewModel.SelectedEquipmentSoftware, "Редактирование связи оборудование-ПО");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEquipmentSoftware == null)
            {
                MessageBox.Show("Выберите связь для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var res = MessageBox.Show("Удалить выбранную связь?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            try
            {
                await _viewModel.DeleteEquipmentSoftwareAsync(
                    _viewModel.SelectedEquipmentSoftware.equipment_id,
                    _viewModel.SelectedEquipmentSoftware.software_id);

                await _viewModel.LoadDataAsync(); // обновить таблицу
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
            _viewModel.FilterEquipmentSoftware();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(EquipmentSoftware item, string title)
        {
            bool isNew = item.equipment_id == 0 && item.software_id == 0;

            var control = new EquipmentSoftwareEditControl(item);
            var dialog = new EditDialog(control, title) { Owner = Window.GetWindow(this) };

            if (dialog.ShowDialog() == true)
            {
                // EditDialog умеет забирать GetEditedItem/GetXxx, но нам проще прямо:
                var edited = control.GetEquipmentSoftware();

                try
                {
                    if (isNew)
                        await _viewModel.AddEquipmentSoftwareAsync(edited);
                    else
                        await _viewModel.UpdateEquipmentSoftwareAsync(edited);

                    await _viewModel.LoadDataAsync(); // обновить таблицу
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}