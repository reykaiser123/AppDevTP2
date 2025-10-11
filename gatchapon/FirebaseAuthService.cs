using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;


namespace gatchapon
{
    public record FirebaseSignInResponse(
        string idToken,
        string email,
        string refreshToken,
        string expiresIn,
        string localId,
        bool registered
    );

    public class FirebaseAuthService
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _apiKey;

        private const string FirebaseApiKey = "AIzaSyDnE-unnTD3dSGWzovUUUjYWvMI4NTR2DQ";

        public FirebaseAuthService()
        {
            try
            {
                var file = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    _apiKey = config?["FirebaseApiKey"] ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading config: {ex.Message}");
                _apiKey = string.Empty;
            }
        }

        public async Task<FirebaseSignInResponse?> RefreshIdTokenAsync(string refreshToken)
        {
            try
            {
                var requestUri = $"https://securetoken.googleapis.com/v1/token?key={_apiKey}";

                var payload = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };

                var response = await _httpClient.PostAsync(requestUri, new FormUrlEncodedContent(payload));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Firebase Token Refresh Error: {error}");
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                // Build response manually since refresh API returns different keys
                return new FirebaseSignInResponse(
                    result["id_token"],
                    "", // email not returned, optional
                    result["refresh_token"],
                    result["expires_in"],
                    result["user_id"],
                    true
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during token refresh: {ex.Message}");
                return null;
            }
        }
        public async Task SignOutAsync()
        {
            SecureStorage.Remove("refresh_token");
            SecureStorage.Remove("user_email");
        }

        public async Task SaveUserSessionAsync(FirebaseSignInResponse user)
        {
            await SecureStorage.SetAsync("refresh_token", user.refreshToken);
            await SecureStorage.SetAsync("user_email", user.email);
        }

        public async Task<FirebaseSignInResponse?> SignInResponseAsync(string email, string password)
        {

            try
            {
                string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}";

                var payload = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsJsonAsync(requestUri, payload);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Firebase Sign-In Error: {error}");
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during Firebase login: {ex.Message}");
                return null;
            }
        }


        public async Task<FirebaseSignInResponse?> SignUpWithEmailPasswordAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("Firebase API key not found.");
                return null;
            }

            string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
            var payload = new { email, password, returnSecureToken = true };

            var response = await _httpClient.PostAsJsonAsync(requestUri, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Firebase Sign-Up Error: {error}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
        }
    }
}
