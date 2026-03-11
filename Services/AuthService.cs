using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using AdminUP.Models;

namespace AdminUP.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>Логин текущего пользователя</summary>
        public string CurrentUser { get; private set; }

        /// <summary>Роль текущего пользователя (admin / teacher / staff)</summary>
        public string CurrentRole { get; private set; }

        /// <summary>ID текущего пользователя в БД</summary>
        public int CurrentUserId { get; private set; }

        /// <summary>Полный объект текущего пользователя</summary>
        public User CurrentUserObject { get; private set; }

        /// <summary>True — пользователь авторизован</summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>True — администратор, видит все данные без фильтрации</summary>
        public bool IsAdmin => CurrentRole == "admin";

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
                var user = users?.Find(u => u.login == login);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                bool passwordValid = false;

                if (!string.IsNullOrWhiteSpace(user.password_hash) && user.password_hash.StartsWith("$2"))
                    passwordValid = BCrypt.Net.BCrypt.Verify(password, user.password_hash);
                else if (!string.IsNullOrWhiteSpace(user.password_hash) && user.password_hash.StartsWith("v1$"))
                    passwordValid = AdminUP.Security.BCrypt.Verify(password, user.password_hash);
                else
                    passwordValid = password == user.password_hash;

                if (!passwordValid)
                {
                    MessageBox.Show("Неверный пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                CurrentUser = user.login;
                CurrentRole = user.role;
                CurrentUserId = user.id;
                CurrentUserObject = user;
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
            CurrentUserObject = null;
            CurrentUserId = 0;
            IsAuthenticated = false;
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
    }
}
