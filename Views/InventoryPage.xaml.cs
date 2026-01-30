using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class InventoryPage : Page
    {
        private readonly InventoryPageViewModel _viewModel;

        public InventoryPage()
        {
            InitializeComponent();
            _viewModel = new InventoryPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
            => await _viewModel.LoadInventoriesAsync();

        private void AddButton_Click(object sender, RoutedEventArgs e)
            => ShowEditDialog(new Inventory(), "Создание инвентаризации");

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventory == null)
            {
                MessageBox.Show("Выберите инвентаризацию для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowEditDialog(_viewModel.SelectedInventory, "Редактирование инвентаризации");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventory == null)
            {
                MessageBox.Show("Выберите инвентаризацию для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await _viewModel.DeleteInventoryAsync(_viewModel.SelectedInventory.Id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e) => _viewModel.FilterInventories();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => EditButton_Click(sender, e);

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отчет пока не реализован.\nДальше подключим ExportService (Excel/Word).",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ShowEditDialog(Inventory item, string title)
        {
            var dlg = new EditDialog(item, title) { Owner = Window.GetWindow(this) };

            if (dlg.ShowDialog() == true && dlg.GetEditedItem() is Inventory edited)
            {
                if (edited.Id == 0)
                    await _viewModel.AddInventoryAsync(edited);
                else
                    await _viewModel.UpdateInventoryAsync(edited.Id, edited);
            }
        }
    }
}
