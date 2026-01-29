using AdminUP.Services;
using AdminUP.Views;
using System.Windows;

namespace AdminUP
{
    public partial class App : Application
    {
        public static ApiService ApiService { get; private set; }
        public static AuthService AuthService { get; private set; }
        public static CacheService CacheService { get; private set; }
        public static ExportService ExportService { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Инициализация сервисов
            ApiService = new ApiService();
            AuthService = new AuthService();
            CacheService = new CacheService();
            ExportService = new ExportService();

            // Показываем окно входа
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                // Показываем главное окно
                var mainWindow = new MainPage();
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            CacheService?.Clear();
        }

        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Произошла ошибка:\n{e.Exception.Message}",
                "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}