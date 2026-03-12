using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using AdminUP.Services;
using AdminUP.Views;

namespace AdminUP
{
    public partial class App : Application
    {
        // ─── Сервисы (синглтоны уровня приложения) ───────────────────────────

        /// <summary>HTTP-клиент к REST API</summary>
        public static ApiService ApiService { get; private set; } = null!;

        /// <summary>Сервис авторизации, хранит CurrentUser / CurrentRole</summary>
        public static AuthService AuthService { get; private set; } = null!;

        /// <summary>Кэш для уменьшения числа запросов к API</summary>
        public static CacheService CacheService { get; private set; } = null!;

        /// <summary>Сервис экспорта актов (DOCX)</summary>
        public static ExportService ExportService { get; private set; } = null!;

        /// <summary>Сервис работы с фотографиями оборудования</summary>
        public static PhotoService PhotoService { get; private set; } = null!;

        /// <summary>Сервис формирования отчёта по сотрудникам (Приложение 4)</summary>
        public static StaffReportService StaffReportService { get; private set; } = null!;

        // ─── Путь к лог-файлу ────────────────────────────────────────────────

        /// <summary>Путь к файлу ошибок рядом с .exe</summary>
        private static readonly string LogPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "errors.log");

        // ─── Startup ─────────────────────────────────────────────────────────

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // ✅ п. 2.5 ТЗ: перехватываем все необработанные исключения
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Приложение не закрывается, когда закрывается LoginWindow
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            ApiService        = new ApiService();
            AuthService       = new AuthService();
            CacheService      = new CacheService();
            ExportService     = new ExportService(ApiService);
            PhotoService      = new PhotoService();
            StaffReportService = new StaffReportService(ApiService);

            var loginWindow = new LoginWindow();
            var ok = loginWindow.ShowDialog() == true;

            if (!ok)
            {
                Shutdown();
                return;
            }

            var mainWindow = new MainPage();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }

        // ─── Глобальные обработчики ошибок ───────────────────────────────────

        /// <summary>
        /// Перехватывает исключения в потоке UI.
        /// Записывает в лог-файл (п. 2.5 ТЗ) и отправляет в таблицу error_logs.
        /// </summary>
        private void App_DispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            LogError(e.Exception, "UI");

            MessageBox.Show(
                $"Произошла непредвиденная ошибка:\n\n{e.Exception.Message}\n\n" +
                $"Подробности записаны в файл {LogPath}",
                "Ошибка приложения",
                MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true; // не даём приложению упасть
        }

        /// <summary>
        /// Перехватывает исключения в фоновых потоках (не UI).
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                LogError(ex, "Background");
        }

        // ─── Логирование ─────────────────────────────────────────────────────

        /// <summary>
        /// Записывает исключение в лог-файл и асинхронно в БД (error_logs).
        /// п. 2.5 ТЗ: "необходимо занести в специальную таблицу БД и записать в лог-файл".
        /// </summary>
        public static void LogError(Exception ex, string source = "")
        {
            try
            {
                // 1. Записываем в файл на диск
                var line =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] " +
                    $"{ex.GetType().Name}: {ex.Message}" + Environment.NewLine +
                    (ex.StackTrace ?? "") + Environment.NewLine +
                    (ex.InnerException != null
                        ? $"  InnerException: {ex.InnerException.Message}" + Environment.NewLine
                        : "") +
                    new string('-', 80) + Environment.NewLine;

                File.AppendAllText(LogPath, line);
            }
            catch
            {
                // если не удалось записать лог — молча игнорируем
            }

            try
            {
                // 2. Асинхронно отправляем в API (не ждём)
                var login = AuthService?.CurrentUser ?? "anonymous";
                _ = ApiService?.AddItemAsync("ErrorLogsController", new Models.ErrorLog
                {
                    method       = source.Length > 10 ? source[..10] : source,
                    path         = ex.GetType().Name,
                    message      = ex.Message,
                    stack_trace  = ex.StackTrace,
                    inner_exception = ex.InnerException?.Message,
                    user_name    = login,
                    created_at   = DateTime.UtcNow
                });
            }
            catch
            {
                // если API недоступен — молча игнорируем
            }
        }
    }
}
