using AdminUP.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class MainPage : Window
    {
        private string _currentPageTitle;
        private bool _closeWithoutConfirm = false;

        /// <summary>Кнопки меню, доступные только администратору</summary>
        private System.Windows.Controls.Control[] AdminOnlyButtons => new System.Windows.Controls.Control[]
        {
            AdminSeparator,
            BtnUsers, BtnClassrooms, BtnConsumables, BtnStatuses,
            BtnEqTypes, BtnModels, BtnConsumableTypes, BtnCharacteristics,
            BtnDevelopers, BtnDirections, BtnSoftware
            // BtnInventory и BtnInventoryItems доступны всем ролям
        };

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

            // Скрываем административные пункты меню для не-админов
            if (!App.AuthService.IsAdmin)
                HideAdminMenuItems();

            // Показываем имя пользователя в заголовке
            UserNameText.Text = $"👤 {App.AuthService.CurrentUserObject?.last_name} {App.AuthService.CurrentUserObject?.first_name}  [{App.AuthService.CurrentRole}]";

            NavigateToPage("EquipmentPage");
        }

        /// <summary>Скрывает пункты меню, недоступные рядовому пользователю</summary>
        private void HideAdminMenuItems()
        {
            // Не-админ видит только: своё оборудование, ПО, историю, сетевые настройки
            foreach (var btn in AdminOnlyButtons)
                btn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void NavigateToPage(string pageName)
        {
            try
            {
                switch (pageName)
                {
                    case "EquipmentHistoryPage":
                        MainFrame.Navigate(new EquipmentHistoryPage());
                        CurrentPageTitle = "📝 История оборудования";
                        break;
                    case "EquipmentSoftwarePage":
                        MainFrame.Navigate(new EquipmentSoftwarePage());
                        CurrentPageTitle = "🔗 Оборудование-ПО";
                        break;
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
                if (pageName == "ExportReport")
                {
                    _ = new AdminUP.Services.ExportService(App.ApiService).ExportEquipmentReportAsync();
                    return;
                }
                if (pageName == "GenerateAct")
                {
                    var actWin = new AdminUP.Views.ActGeneratorWindow(App.ApiService) { Owner = this };
                    actWin.ShowDialog();
                    return;
                }
                if (pageName == "StaffReport")
                {
                    _ = App.StaffReportService.GenerateAsync();
                    return;
                }
                if (pageName == "ImportEquipment")
                {
                    _ = RunImportAsync();
                    return;
                }
                if (pageName == "DownloadTemplate")
                {
                    App.ImportService.DownloadTemplate();
                    return;
                }
                NavigateToPage(pageName);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
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
            if (MainFrame.Content is Page currentPage)
            {
                var loadedMethod = currentPage.GetType().GetMethod("Page_Loaded");
                loadedMethod?.Invoke(currentPage, new object[] { currentPage, new RoutedEventArgs() });
            }
        }

        private void SwitchUserButton_Click(object sender, RoutedEventArgs e)
        {
            App.AuthService.Logout();

            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            if (loginWindow.ShowDialog() == true)
            {
                UserNameText.Text = $"{App.AuthService.CurrentUserObject?.last_name} " +
                                    $"{App.AuthService.CurrentUserObject?.first_name}  " +
                                    $"[{App.AuthService.CurrentRole}]";
                if (!App.AuthService.IsAdmin)
                    HideAdminMenuItems();
                else
                    ShowAdminMenuItems();
                NavigateToPage("EquipmentPage");
                App.CacheService.Clear();
            }
            else
            {
                // Отменил — снова запрашиваем вход
                var loginWindow2 = new LoginWindow();
                loginWindow2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (loginWindow2.ShowDialog() != true)
                {
                    _closeWithoutConfirm = true;
                    Close();
                    return;
                }
                UserNameText.Text = $"{App.AuthService.CurrentUserObject?.last_name} " +
                                    $"{App.AuthService.CurrentUserObject?.first_name}  " +
                                    $"[{App.AuthService.CurrentRole}]";
                if (!App.AuthService.IsAdmin) HideAdminMenuItems();
                else ShowAdminMenuItems();
                App.CacheService.Clear();
            }
        }

        private void ShowAdminMenuItems()
        {
            foreach (var btn in AdminOnlyButtons)
                btn.Visibility = System.Windows.Visibility.Visible;
        }

        void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.AuthService.Logout();

            _closeWithoutConfirm = true;   // ✅ важно
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            Close(); // теперь confirm не появится
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closeWithoutConfirm) return;

            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                e.Cancel = true;
        }


        private async System.Threading.Tasks.Task RunImportAsync()
        {
            try
            {
                var (added, skipped, errors, log) = await App.ImportService.ImportFromFileAsync();
                if (added == 0 && skipped == 0 && errors == 0) return;
                var errorLines = log.Where(l => l.Contains("❌")).Take(5).ToList();
                var errDetail = errorLines.Count > 0 ? "\n\n" + string.Join("\n", errorLines) : "";
                var summary = $"Импорт завершён:\n✅ Добавлено: {added}\n⚠️ Пропущено: {skipped}\n❌ Ошибок: {errors}{errDetail}";
                System.Windows.MessageBox.Show(summary, "Импорт",
                    System.Windows.MessageBoxButton.OK,
                    errors > 0 ? System.Windows.MessageBoxImage.Warning : System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка импорта:\n" + ex.Message,
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
