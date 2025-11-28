using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using gatchapon.Models; // Ensure this is here to use UserTask

namespace gatchapon
{
    public class FirebaseDatabaseService
    {
        private readonly HttpClient _httpClient = new();
        private const string DatabaseUrl = "https://gatchapon-d7cd9-default-rtdb.firebaseio.com/";

        // ---------------------------------------------------------
        //  1. EXISTING USER METHODS
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        //  2. NEW: SAVE TASK METHOD (Fixes your error!)
        // ---------------------------------------------------------
        public async Task<bool> SaveUserTaskAsync(string userId, UserTask task)
        {
            try
            {
                // If TaskId is empty, it's a NEW task -> Use POST (Firebase generates ID)
                if (string.IsNullOrEmpty(task.TaskId))
                {
                    var response = await _httpClient.PostAsJsonAsync($"{DatabaseUrl}tasks/{userId}.json", task);
                    return response.IsSuccessStatusCode;
                }
                // If TaskId exists, it's an UPDATE -> Use PUT (To specific ID)
                else
                {
                    var response = await _httpClient.PutAsJsonAsync($"{DatabaseUrl}tasks/{userId}/{task.TaskId}.json", task);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveUserTaskAsync error: {ex.Message}");
                return false;
            }
        }
    }
}