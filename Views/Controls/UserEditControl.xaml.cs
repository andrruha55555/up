using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdminUP.Models;

namespace AdminUP.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserEditControl.xaml
    /// </summary>
    public partial class UserEditControl : UserControl
    {
        private User _user;
        public UserEditControl(User user = null)
        {
            InitializeComponent();
            _user = user ?? new User();
            DataContext = _user;
        }

        public User GetUser()
        {
            return _user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(_user.Login) ||
                string.IsNullOrWhiteSpace(_user.LastName) ||
                string.IsNullOrWhiteSpace(_user.FirstName) ||
                string.IsNullOrWhiteSpace(_user.Email))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Если это новый пользователь, хэшируем пароль
            if (_user.Id == 0 && !string.IsNullOrEmpty(PasswordBox.Password))
            {
                _user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password);
            }

            // Закрываем окно с результатом OK
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = false;
            Window.GetWindow(this).Close();
        }
    }
}
