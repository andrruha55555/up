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
            if (string.IsNullOrWhiteSpace(_user.role))
                _user.role = "staff";
            DataContext = _user;
        }

        public User GetUser() => _user;
        public object GetEditedItem() => _user;

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(_user.login))
            {
                MessageBox.Show("Введите логин.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(_user.last_name))
            {
                MessageBox.Show("Введите фамилию.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (_user.id == 0)
            {
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Введите пароль.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                _user.password_hash = PasswordHasher.Hash(PasswordBox.Password);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                    _user.password_hash = PasswordHasher.Hash(PasswordBox.Password);
            }
            return true;
        }
    }
}
