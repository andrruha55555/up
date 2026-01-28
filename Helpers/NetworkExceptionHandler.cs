using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Helpers
{
    public static class NetworkExceptionHandler
    {
        public static async Task<T> HandleRequestAsync<T>(Func<Task<T>> request, string operationName)
        {
            try
            {
                return await request();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка сети при {operationName}: {ex.Message}\n" +
                    "Проверьте подключение к серверу.", "Ошибка сети",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return default;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show($"Превышено время ожидания при {operationName}", "Таймаут",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return default;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при {operationName}: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return default;
            }
        }

        public static async Task<bool> HandleRequestAsync(Func<Task> request, string operationName)
        {
            try
            {
                await request();
                return true;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка сети при {operationName}: {ex.Message}", "Ошибка сети",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show($"Превышено время ожидания при {operationName}", "Таймаут",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при {operationName}: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
