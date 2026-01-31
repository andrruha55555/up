using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class ConsumableTypePage : Page
    {
        private ConsumableTypePageViewModel _viewModel;

        public ConsumableTypePage()
        {
            InitializeComponent();

            _viewModel = new ConsumableTypePageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadConsumableTypesAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newType = new ConsumableType();
            ShowEditDialog(newType, "Добавление типа расходника");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedConsumableType != null)
            {
                ShowEditDialog(_viewModel.SelectedConsumableType, "Редактирование типа расходника");
            }
            else
            {
                MessageBox.Show("Выберите тип расходника для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedConsumableType != null)
            {
                await _viewModel.DeleteConsumableTypeAsync(_viewModel.SelectedConsumableType.id);
            }
            else
            {
                MessageBox.Show("Выберите тип расходника для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterConsumableTypes();
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

        private async void ShowEditDialog(ConsumableType consumableType, string title)
        {
            var control = new ConsumableTypeEditControl(consumableType);

            var editDialog = new EditDialog(control, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedType = control.GetConsumableType();

                if (editedType != null)
                {
                    if (editedType.id == 0)
                        await _viewModel.AddConsumableTypeAsync(editedType);
                    else
                        await _viewModel.UpdateConsumableTypeAsync(editedType.id, editedType);
                }
            }
        }
    }
}
