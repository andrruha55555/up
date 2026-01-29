using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class ConsumablePage : Page
    {
        private ConsumablePageViewModel _viewModel;

        public ConsumablePage()
        {
            InitializeComponent();

            _viewModel = new ConsumablePageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadConsumablesAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newConsumable = new Consumable();
            ShowEditDialog(newConsumable, "Добавление расходника");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedConsumable != null)
            {
                ShowEditDialog(_viewModel.SelectedConsumable, "Редактирование расходника");
            }
            else
            {
                MessageBox.Show("Выберите расходник для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedConsumable != null)
            {
                await _viewModel.DeleteConsumableAsync(_viewModel.SelectedConsumable.Id);
            }
            else
            {
                MessageBox.Show("Выберите расходник для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterConsumables();
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

        private async void ShowEditDialog(Consumable consumable, string title)
        {
            var editDialog = new EditDialog(consumable, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedConsumable = editDialog.GetEditedItem() as Consumable;
                if (editedConsumable != null)
                {
                    if (editedConsumable.Id == 0)
                        await _viewModel.AddConsumableAsync(editedConsumable);
                    else
                        await _viewModel.UpdateConsumableAsync(editedConsumable.Id, editedConsumable);
                }
            }
        }
    }
}