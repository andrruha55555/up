using System.Windows;
using AdminUP.Services;

namespace AdminUP.Views
{
    public partial class LoginWindow : Window
    {
        private AuthService _authService;
        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoginButton.IsEnabled = false;

            var success = await _authService.LoginAsync(login, password);

            LoginButton.IsEnabled = true;

            if (success)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}