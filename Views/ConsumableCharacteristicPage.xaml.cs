using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class ConsumableCharacteristicPage : Page
    {
        private ConsumableCharacteristicPageViewModel _viewModel;

        public ConsumableCharacteristicPage()
        {
            InitializeComponent();

            _viewModel = new ConsumableCharacteristicPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;

            // Привязка ComboBox
            ConsumableComboBox.ItemsSource = _viewModel.ConsumableList;
            ConsumableComboBox.DisplayMemberPath = "Name";
            ConsumableComboBox.SelectedValuePath = "Id";
            ConsumableComboBox.SelectedValue = _viewModel.SelectedConsumableId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newCharacteristic = new ConsumableCharacteristic();
            ShowEditDialog(newCharacteristic, "Добавление характеристики");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedCharacteristic != null)
            {
                ShowEditDialog(_viewModel.SelectedCharacteristic, "Редактирование характеристики");
            }
            else
            {
                MessageBox.Show("Выберите характеристику для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedCharacteristic != null)
            {
                await _viewModel.DeleteCharacteristicAsync(_viewModel.SelectedCharacteristic.Id);
            }
            else
            {
                MessageBox.Show("Выберите характеристику для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterCharacteristics();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void ConsumableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConsumableComboBox.SelectedValue is int selectedId)
            {
                _viewModel.SelectedConsumableId = selectedId;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(ConsumableCharacteristic characteristic, string title)
        {
            var editDialog = new EditDialog(characteristic, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedCharacteristic = editDialog.GetEditedItem() as ConsumableCharacteristic;
                if (editedCharacteristic != null)
                {
                    if (editedCharacteristic.Id == 0)
                        await _viewModel.AddCharacteristicAsync(editedCharacteristic);
                    else
                        await _viewModel.UpdateCharacteristicAsync(editedCharacteristic.Id, editedCharacteristic);
                }
            }
        }
    }
}