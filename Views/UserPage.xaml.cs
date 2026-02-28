using System.Windows;
using System.Windows.Controls;
using AdminUP.Models;
using AdminUP.ViewModels;

namespace AdminUP.Views
{
    public partial class UserPage : Page
    {
        private readonly UserPageViewModel _viewModel;

        public UserPage()
        {
            InitializeComponent();
            _viewModel = new UserPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadUsersAsync();
            _viewModel.FilterUsers();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = SearchTextBox.Text;
            _viewModel.FilterUsers();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
            _viewModel.FilterUsers();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SearchText = SearchTextBox.Text;
            _viewModel.FilterUsers();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEditDialog(new User(), "Добавление пользователя");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ShowEditDialog(_viewModel.SelectedUser, "Редактирование пользователя");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await _viewModel.DeleteUserAsync(_viewModel.SelectedUser.id);
        }

        private async void ShowEditDialog(User user, string title)
        {
            var dialog = new EditDialog(new Views.Controls.UserEditControl(user), title);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                if (dialog.GetEditedItem() is User editedUser)
                {
                    if (editedUser.id == 0)
                        await _viewModel.AddUserAsync(editedUser);
                    else
                        await _viewModel.UpdateUserAsync(editedUser.id, editedUser);
                }
            }
        }
    }
}