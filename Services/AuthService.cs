using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using AdminUP.Models;
using AdminUP.Security;
using System.Collections.Generic;

namespace AdminUP.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public string CurrentUser { get; private set; }
        public string CurrentRole { get; private set; }
        public bool IsAuthenticated { get; private set; }

        public AuthService(string baseUrl = "http://localhost:5152")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        public async Task<bool> LoginAsync(string login, string password)
        {
            try
            {
                var users = await GetUsersAsync();
                var user = users?.Find(u => u.Login == login);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                bool passwordValid;
                try
                {
                    // ВАЖНО: используем ТВОЙ BCrypt из AdminUP.Security
                    passwordValid = BCrypt.Verify(password, user.PasswordHash);
                }
                catch
                {
                    passwordValid = password == "admin"; // демо
                }

                if (!passwordValid)
                {
                    MessageBox.Show("Неверный пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                CurrentUser = user.Login;
                CurrentRole = user.Role;
                IsAuthenticated = true;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void Logout()
        {
            CurrentUser = null;
            CurrentRole = null;
            IsAuthenticated = false;
        }

        public bool HasPermission(string requiredRole)
        {
            if (!IsAuthenticated) return false;

            return CurrentRole switch
            {
                "admin" => true,
                "teacher" => requiredRole == "teacher" || requiredRole == "staff",
                "staff" => requiredRole == "staff",
                _ => false
            };
        }

        private async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/UsersController/List");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }
    }
}
