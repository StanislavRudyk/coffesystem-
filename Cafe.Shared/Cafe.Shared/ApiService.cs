using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cafe.Shared.Models;

namespace Cafe.Shared.Services
{
    public class ApiService
    {
        private static ApiService _instance;
        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:5000/api/";
        
        public User CurrentUser { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private ApiService()
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public static ApiService Instance
        {
            get
            {
                if (_instance == null) _instance = new ApiService();
                return _instance;
            }
        }

        private async Task<T> GetAsync<T>(string url) where T : new()
        {
            try
            {
                var json = await _http.GetStringAsync(url);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"API Error ({url}): {ex.Message}");
                return new T(); 
            }
        }

        private async Task<bool> PostAsync<T>(string url, T body)
        {
            try
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync(url, content);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"API Post Error ({url}): {ex.Message}");
                return false; 
            }
        }

        private async Task<bool> PutAsync<T>(string url, T body)
        {
            try
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PutAsync(url, content);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"API Put Error ({url}): {ex.Message}");
                return false; 
            }
        }

        public Task<List<Product>> GetProductsAsync() => GetAsync<List<Product>>("products");

        public Task<List<Category>> GetCategoriesAsync() => GetAsync<List<Category>>("categories");

        public Task<List<Order>> GetOrdersAsync() => GetAsync<List<Order>>("orders");

        public Task<List<User>> GetUsersAsync() => GetAsync<List<User>>("users");

        public async Task<User> LoginAsync(string username, string password)
        {
            try
            {
                var body = new { Username = username, Password = password };
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var resp = await _http.PostAsync("users/login", content);
                if (resp.IsSuccessStatusCode)
                {
                    var respJson = await resp.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<User>(respJson, _jsonOptions);
                    if (user != null)
                    {
                        CurrentUser = user;
                        return user;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Login Error: {ex.Message}");
                return null;
            }
        }

        public Task<bool> CreateOrderAsync(Order order) => PostAsync("orders", order);

        public Task<bool> CreateProductAsync(Product product) => PostAsync("products", product);

        public Task<bool> UpdateProductAsync(Product product) => PutAsync($"products/{product.Id}", product);

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var resp = await _http.DeleteAsync($"products/{id}");
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var resp = await _http.DeleteAsync($"orders/{id}");
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public Task<bool> CreateUserAsync(User user) => PostAsync("users", user);
        public Task<bool> UpdateUserAsync(User user) => PutAsync($"users/{user.Id}", user);
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var resp = await _http.DeleteAsync($"users/{id}");
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
