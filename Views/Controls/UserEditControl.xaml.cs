using System.Windows;
using System.Windows.Controls;
using AdminUP.Models;
using AdminUP.Security;

namespace AdminUP.Views.Controls
{
    public partial class UserEditControl : UserControl
    {
        private readonly User _user;

        public UserEditControl(User? user = null)
        {
            InitializeComponent();
            _user = user ?? new User();
            DataContext = _user;
        }

        public User GetUser() => _user;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_user.login) ||
                string.IsNullOrWhiteSpace(_user.last_name) ||
                string.IsNullOrWhiteSpace(_user.first_name) ||
                string.IsNullOrWhiteSpace(_user.email))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_user.id == 0)
            {
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Для нового пользователя пароль обязателен!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _user.password_hash = PasswordHasher.Hash(PasswordBox.Password);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                    _user.password_hash = PasswordHasher.Hash(PasswordBox.Password);
            }

            var wnd = Window.GetWindow(this);
            if (wnd != null)
            {
                wnd.DialogResult = true;
                wnd.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this);
            if (wnd != null)
            {
                wnd.DialogResult = false;
                wnd.Close();
            }
        }
    }
}
