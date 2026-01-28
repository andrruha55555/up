using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AdminUP.Helpers;

namespace AdminUP.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(string baseUrl = "http://localhost:5152")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            return await NetworkExceptionHandler.HandleRequestAsync(async () =>
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/{endpoint}/List");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }, $"получении списка из {endpoint}");
        }

        public async Task<T> GetItemAsync<T>(string endpoint, int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/{endpoint}/Item?id={id}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting item from {endpoint}: {ex.Message}");
                return default;
            }
        }

        public async Task<bool> AddItemAsync<T>(string endpoint, T item)
        {
            try
            {
                var json = JsonSerializer.Serialize(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/{endpoint}/Add", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item to {endpoint}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateItemAsync<T>(string endpoint, int id, T item)
        {
            try
            {
                var json = JsonSerializer.Serialize(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/api/{endpoint}/Update?id={id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating item in {endpoint}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteItemAsync(string endpoint, int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/{endpoint}/Delete?id={id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting item from {endpoint}: {ex.Message}");
                return false;
            }
        }
    }
}