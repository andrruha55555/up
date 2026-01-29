using AdminUP.Models;
using AdminUP.ViewModels;
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
            var editDialog = new EditDialog(status, title);
            if (editDialog.ShowDialog() == true && editDialog.GetEditedItem() is Status editedStatus)
            {
                if (editedStatus.Id == 0)
                    await _viewModel.AddStatusAsync(editedStatus);
                else
                    await _viewModel.UpdateStatusAsync(editedStatus.Id, editedStatus);
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
                await deleteFunc((selected as dynamic).Id);
        }

        private void ClearSearch()
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }
    }
}