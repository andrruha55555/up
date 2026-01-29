using System;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class MainPage : Window
    {
        private string _currentPageTitle;

        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set
            {
                _currentPageTitle = value;
                // Реализовать INotifyPropertyChanged если нужно
            }
        }

        public MainPage()
        {
            InitializeComponent();
            NavigateToPage("EquipmentPage");
        }

        private void NavigateToPage(string pageName)
        {
            try
            {
                switch (pageName)
                {
                    case "EquipmentPage":
                        MainFrame.Navigate(new EquipmentPage());
                        CurrentPageTitle = "📦 Оборудование";
                        break;
                    case "UserPage":
                        MainFrame.Navigate(new UserPage());
                        CurrentPageTitle = "👥 Пользователи";
                        break;
                    case "ClassroomPage":
                        MainFrame.Navigate(new ClassroomPage());
                        CurrentPageTitle = "🏫 Аудитории";
                        break;
                    case "ConsumablePage":
                        MainFrame.Navigate(new ConsumablePage());
                        CurrentPageTitle = "🖨️ Расходные материалы";
                        break;
                    case "StatusPage":
                        MainFrame.Navigate(new StatusPage());
                        CurrentPageTitle = "📈 Статусы оборудования";
                        break;
                    case "EquipmentTypePage":
                        MainFrame.Navigate(new EquipmentTypePage());
                        CurrentPageTitle = "💻 Типы оборудования";
                        break;
                    case "ModelPage":
                        MainFrame.Navigate(new ModelPage());
                        CurrentPageTitle = "🖥️ Модели оборудования";
                        break;
                    case "ConsumableTypePage":
                        MainFrame.Navigate(new ConsumableTypePage());
                        CurrentPageTitle = "📊 Типы расходных материалов";
                        break;
                    case "ConsumableCharacteristicPage":
                        MainFrame.Navigate(new ConsumableCharacteristicPage());
                        CurrentPageTitle = "📝 Характеристики расходных материалов";
                        break;
                    case "DeveloperPage":
                        MainFrame.Navigate(new DeveloperPage());
                        CurrentPageTitle = "👨‍💻 Разработчики ПО";
                        break;
                    case "DirectionPage":
                        MainFrame.Navigate(new DirectionPage());
                        CurrentPageTitle = "🎯 Направления";
                        break;
                    case "SoftwarePage":
                        MainFrame.Navigate(new SoftwarePage());
                        CurrentPageTitle = "🛠️ Программное обеспечение";
                        break;
                    case "InventoryPage":
                        MainFrame.Navigate(new InventoryPage());
                        CurrentPageTitle = "📋 Инвентаризации";
                        break;
                    case "InventoryItemPage":
                        MainFrame.Navigate(new InventoryItemPage());
                        CurrentPageTitle = "✓ Элементы инвентаризации";
                        break;
                    case "NetworkSettingPage":
                        MainFrame.Navigate(new NetworkSettingPage());
                        CurrentPageTitle = "🌐 Сетевые настройки";
                        break;
                    default:
                        MainFrame.Navigate(new EquipmentPage());
                        CurrentPageTitle = "📦 Оборудование";
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки страницы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageName)
            {
                NavigateToPage(pageName);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем метод добавления текущей страницы
            if (MainFrame.Content is Page currentPage)
            {
                var addMethod = currentPage.GetType().GetMethod("AddButton_Click");
                addMethod?.Invoke(currentPage, new object[] { sender, e });
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is Page currentPage)
            {
                var editMethod = currentPage.GetType().GetMethod("EditButton_Click");
                editMethod?.Invoke(currentPage, new object[] { sender, e });
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is Page currentPage)
            {
                var deleteMethod = currentPage.GetType().GetMethod("DeleteButton_Click");
                deleteMethod?.Invoke(currentPage, new object[] { sender, e });
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is Page currentPage)
            {
                var searchMethod = currentPage.GetType().GetMethod("SearchButton_Click");
                searchMethod?.Invoke(currentPage, new object[] { sender, e });
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is Page currentPage)
            {
                var clearMethod = currentPage.GetType().GetMethod("ClearButton_Click");
                clearMethod?.Invoke(currentPage, new object[] { sender, e });
            }
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем текущую страницу
            if (MainFrame.Content is Page currentPage)
            {
                var loadedMethod = currentPage.GetType().GetMethod("Page_Loaded");
                loadedMethod?.Invoke(currentPage, new object[] { currentPage, new RoutedEventArgs() });
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.AuthService.Logout();

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализация
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }
}