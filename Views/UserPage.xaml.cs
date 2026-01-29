using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class UserPage : Page
    {
        private UserPageViewModel _viewModel;

        public UserPage()
        {
            InitializeComponent();

            _viewModel = new UserPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadUsersAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newUser = new User();
            ShowEditDialog(newUser, "Добавление пользователя");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedUser != null)
            {
                ShowEditDialog(_viewModel.SelectedUser, "Редактирование пользователя");
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedUser != null)
            {
                await _viewModel.DeleteUserAsync(_viewModel.SelectedUser.Id);
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterUsers();
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

        private async void ShowEditDialog(User user, string title)
        {
            var editDialog = new EditDialog(user, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedUser = editDialog.GetEditedItem() as User;
                if (editedUser != null)
                {
                    if (editedUser.Id == 0)
                        await _viewModel.AddUserAsync(editedUser);
                    else
                        await _viewModel.UpdateUserAsync(editedUser.Id, editedUser);
                }
            }
        }
    }
}