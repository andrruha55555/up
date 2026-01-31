using System.Windows;
using AdminUP.Services;
using AdminUP.Views;

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
            // ✅ чтобы приложение НЕ закрывалось когда закроется LoginWindow
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            ApiService = new ApiService();
            AuthService = new AuthService();
            CacheService = new CacheService();
            ExportService = new ExportService();

            var loginWindow = new LoginWindow();
            var ok = loginWindow.ShowDialog() == true;

            if (!ok)
            {
                Shutdown();
                return;
            }

            var mainWindow = new MainPage();

            // ✅ назначаем главное окно
            MainWindow = mainWindow;

            // ✅ теперь закрытие приложения будет когда закроется MainWindow
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            mainWindow.Show();
        }
    }
}
