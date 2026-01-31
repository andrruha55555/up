using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminUP.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(string baseUrl = "http://localhost:5152")
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = new HttpClient();
        }

        // ====== PUBLIC API ======

        public async Task<List<T>?> GetListAsync<T>(string controller)
        {
            var url = BuildUrl(controller, "List");
            using var resp = await _http.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw BuildHttpException("GET", url, resp, content);

            if (string.IsNullOrWhiteSpace(content))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(content, JsonOptions) ?? new List<T>();
        }

        public async Task<bool> AddItemAsync<T>(string controller, T item)
        {
            var url = BuildUrl(controller, "Add");
            var json = JsonSerializer.Serialize(item, JsonOptions);
            using var resp = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw BuildHttpException("POST", url, resp, content);

            return true;
        }

        public async Task<bool> UpdateItemAsync<T>(string controller, int id, T item)
        {
            var url = BuildUrl(controller, $"Update/{id}");
            var json = JsonSerializer.Serialize(item, JsonOptions);
            using var resp = await _http.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw BuildHttpException("PUT", url, resp, content);

            return true;
        }

        public async Task<bool> DeleteItemAsync(string controller, int id)
        {
            var url = BuildUrl(controller, $"Delete/{id}");
            using var resp = await _http.DeleteAsync(url);

            var content = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw BuildHttpException("DELETE", url, resp, content);

            return true;
        }

        // ====== HELPERS ======

        private string BuildUrl(string controller, string actionPath)
        {
            var c = NormalizeController(controller);
            return $"{_baseUrl}/api/{c}/{actionPath}";
        }

        private static string NormalizeController(string controller)
        {
            if (string.IsNullOrWhiteSpace(controller))
                throw new ArgumentException("controller пустой");

            var c = controller.Trim();

            // если передали "api/UsersController" или "/api/UsersController"
            c = c.Replace("\\", "/");
            if (c.StartsWith("/")) c = c[1..];
            if (c.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
                c = c["api/".Length..];

            // если забыли суффикс Controller
            if (!c.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                c += "Controller";

            return c;
        }

        private static Exception BuildHttpException(string method, string url, HttpResponseMessage resp, string body)
        {
            var msg =
                $"{method} {url}\n" +
                $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}\n" +
                $"BODY:\n{body}";
            return new Exception(msg);
        }
    }
}
