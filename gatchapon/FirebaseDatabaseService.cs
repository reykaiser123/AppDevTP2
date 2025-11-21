using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;


namespace gatchapon
{
    public class FirebaseDatabaseService
    {
        private readonly HttpClient _httpClient = new();
        private const string DatabaseUrl = "https://gatchapon-d7cd9-default-rtdb.firebaseio.com/"; 
        
        public async Task<bool> SaveUserAsync(string userId, object userData)
        {
            try
            {
                string url = $"{DatabaseUrl}users/{userId}.json";
                var response = await _httpClient.PutAsJsonAsync(url, userData);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveUserAsync error: {ex.Message}");
                return false;
            }
        }

        // Get user data
        public async Task<T?> GetUserAsync<T>(string userId)
        {
            try
            {
                string url = $"{DatabaseUrl}users/{userId}.json";
                return await _httpClient.GetFromJsonAsync<T>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserAsync error: {ex.Message}");
                return default;
            }
        }
        public async Task<bool> UpdateUserFieldAsync(string userId, string fieldName, object value)
        {
            try
            {
                string url = $"{DatabaseUrl}users/{userId}/{fieldName}.json";
                var response = await _httpClient.PutAsJsonAsync(url, value);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateUserFieldAsync error: {ex.Message}");
                return false;
            }
        }

    }
}
