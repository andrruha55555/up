using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;   // ✅ важно: StatusEditControl
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class StatusPage : Page
    {
        private StatusPageViewModel _viewModel;

        public StatusPage()
        {
            InitializeComponent();
            _viewModel = new StatusPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) => await _viewModel.LoadStatusesAsync();
        private void AddButton_Click(object sender, RoutedEventArgs e) => ShowEditDialog(new Status(), "Добавление статуса");
        private void EditButton_Click(object sender, RoutedEventArgs e) => ShowEditIfSelected(_viewModel.SelectedStatus, "Редактирование статуса");
        private async void DeleteButton_Click(object sender, RoutedEventArgs e) => await DeleteIfSelected(_viewModel.SelectedStatus, _viewModel.DeleteStatusAsync);
        private void SearchButton_Click(object sender, RoutedEventArgs e) => _viewModel.FilterStatuses();
        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearSearch();
        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => EditButton_Click(sender, e);

        private async void ShowEditDialog(Status status, string title)
        {
            // ✅ EditDialog принимает UserControl
            var control = new StatusEditControl(status);
            var editDialog = new EditDialog(control, title);

            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedStatus = control.GetStatus();
                if (editedStatus != null)
                {
                    if (editedStatus.id == 0)
                        await _viewModel.AddStatusAsync(editedStatus);
                    else
                        await _viewModel.UpdateStatusAsync(editedStatus.id, editedStatus);
                }
            }
        }

        private void ShowEditIfSelected(object selected, string title)
        {
            if (selected == null)
                MessageBox.Show("Выберите элемент для редактирования", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                ShowEditDialog(selected as Status, title);
        }

        private async Task DeleteIfSelected<T>(T selected, Func<int, Task<bool>> deleteFunc) where T : class
        {
            if (selected == null)
                MessageBox.Show("Выберите элемент для удаления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                await deleteFunc((selected as dynamic).id);
        }

        private void ClearSearch()
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }
    }
}
