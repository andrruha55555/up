using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class InventoryItemPage : Page
    {
        private InventoryItemPageViewModel _viewModel;

        public InventoryItemPage()
        {
            InitializeComponent();

            _viewModel = new InventoryItemPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;

            // Привязка ComboBox
            InventoryComboBox.ItemsSource = _viewModel.InventoryList;
            InventoryComboBox.DisplayMemberPath = "Name";
            InventoryComboBox.SelectedValuePath = "Id";
            InventoryComboBox.SelectedValue = _viewModel.SelectedInventoryId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new InventoryItem();
            ShowEditDialog(newItem, "Добавление элемента инвентаризации");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventoryItem != null)
            {
                ShowEditDialog(_viewModel.SelectedInventoryItem, "Редактирование элемента инвентаризации");
            }
            else
            {
                MessageBox.Show("Выберите элемент инвентаризации для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventoryItem != null)
            {
                await _viewModel.DeleteInventoryItemAsync(_viewModel.SelectedInventoryItem.id);
            }
            else
            {
                MessageBox.Show("Выберите элемент инвентаризации для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterInventoryItems();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void InventoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InventoryComboBox.SelectedValue is int selectedId)
            {
                _viewModel.SelectedInventoryId = selectedId;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(InventoryItem inventoryItem, string title)
        {
            var control = new InventoryItemEditControl(inventoryItem);

            var editDialog = new EditDialog(control, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedItem = control.GetInventoryItem();

                if (editedItem.id == 0)
                    await _viewModel.AddInventoryItemAsync(editedItem);
                else
                    await _viewModel.UpdateInventoryItemAsync(editedItem.id, editedItem);
            }
        }
    }
}
