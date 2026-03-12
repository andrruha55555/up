using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdminUP.Models;
using AdminUP.Security;

namespace AdminUP.Views.Controls
{
    /// <summary>
    /// Контрол редактирования пользователя.
    /// Обязательные поля (п. 1.9 ТЗ): логин, пароль (при создании), фамилия.
    /// Email — необязателен. Пароль скрыт символом * (п. 2.8 ТЗ).
    /// </summary>
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

        /// <summary>Возвращает редактируемого пользователя.</summary>
        public User GetUser() => _user;

        /// <summary>Возвращает редактируемый объект (универсальный интерфейс).</summary>
        public object GetEditedItem() => _user;

        /// <summary>
        /// Валидация: логин, пароль (при создании), фамилия — обязательны.
        /// Email — НЕ обязателен (п. 1.9 ТЗ).
        /// </summary>
        public bool Validate()
        {
            // Логин обязателен
            if (string.IsNullOrWhiteSpace(_user.login))
            {
                MessageBox.Show("Логин обязателен для заполнения!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_user.login.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Фамилия обязательна
            if (string.IsNullOrWhiteSpace(_user.last_name))
            {
                MessageBox.Show("Фамилия обязательна для заполнения!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Роль обязательна
            if (string.IsNullOrWhiteSpace(_user.role))
            {
                MessageBox.Show("Выберите роль пользователя!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Пароль — обязателен только при создании нового пользователя
            if (_user.id == 0)
            {
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Для нового пользователя пароль обязателен!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                _user.password_hash = PasswordHasher.Hash(PasswordBox.Password);
            }
            else
            {
                // При редактировании — хэшируем только если введён новый пароль
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                    _user.password_hash = PasswordHasher.Hash(PasswordBox.Password);
            }

            // Email — необязателен, но если указан — проверяем формат
            if (!string.IsNullOrWhiteSpace(_user.email) &&
                !Regex.IsMatch(_user.email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Указан некорректный формат email!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ограничение ввода телефона: только цифры, пробелы, +, -, (, ).
        /// </summary>
        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d\s\+\-\(\)]");
        }
    }
}
