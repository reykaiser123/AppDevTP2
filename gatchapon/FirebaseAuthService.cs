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
        private const string DatabaseUrl = "https://gatchapon-d7cd9-default-rtdb.firebaseio.com/";
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
            try
            {
                string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}";
                var payload = new { email, password, returnSecureToken = true };

                var response = await _httpClient.PostAsJsonAsync(requestUri, payload);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Firebase Sign-Up Response: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
#if DEBUG
                    await Application.Current.MainPage.DisplayAlert("Firebase Sign-Up Error", responseBody, "OK");
#endif
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
            }
            catch (Exception ex)
            {
#if DEBUG
                await Application.Current.MainPage.DisplayAlert("Exception", ex.Message, "OK");
#endif
                Console.WriteLine($"Exception during sign-up: {ex.Message}");
                return null;
            }
        }

        //for logout nilagay ko pwede din tangalin if may possible error - (oswell)
        public async Task LogOut()
        {
                       try
            {
                SecureStorage.Remove("refresh_token");
                SecureStorage.Remove("user_email");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
            }
        }

        //checks if user is logged in
        public async Task<bool> IsUserLoggedInAsync()
        {
            String refreshToken = await SecureStorage.GetAsync("refresh_token");
            return !string.IsNullOrEmpty(refreshToken);
        }
        public async Task<string?> GetCurrentIDTokenAsync()
        {
            string refreshToken = await SecureStorage.GetAsync("refresh_token");
            if(string.IsNullOrEmpty(refreshToken)) return null;

            var refreshedResponse = await RefreshIdTokenAsync(refreshToken);
            return refreshedResponse?.idToken;
        }
        public async Task<string?> GetCurrentLocalIdAsync()
        {
            string refreshToken = await SecureStorage.GetAsync("refresh_token");
            if(string.IsNullOrEmpty(refreshToken)) return null;
            var refreshedResponse = await RefreshIdTokenAsync(refreshToken);
            return refreshedResponse?.localId;
        }

        // for change username in profileSettings 
      public async Task<bool> UpdateUserAsync(string UserId, object userData)
        {
            try
            {
                string url = $"{DatabaseUrl}users/{UserId}.json";

                var response = await _httpClient.PatchAsJsonAsync(url, userData);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateUserAsync error: {ex.Message}");
                return false;
            }
        }
        //for change email
        public async Task<bool> UpdateEmailAsync(string newEmail)
        {
            string? idToken = await GetCurrentIDTokenAsync();
            if(idToken == null)
            {
                Console.WriteLine("UpdateEmail Error: Could not retrive ID token.");
                return false;
            }
            string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key=AIzaSyDnE-unnTD3dSGWzovUUUjYWvMI4NTR2DQ";

            var payload = new
            {
                idToken,
                email = newEmail,
                returnSecureToken = true
            };
            var response = await _httpClient.PostAsJsonAsync(requestUri, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Firebase Update Email Error: {error}");
                return false;
            }
            Console.WriteLine($"Email successfully updated to: {newEmail}. Verification link may have been sent.");
            await SecureStorage.SetAsync("user_email", newEmail);
            return true;
        }
        //for change password
        public async Task<bool> UpdatePasswordAsync(string newPassword)
        {
            
            string? idToken = await GetCurrentIDTokenAsync();
            if (idToken == null)
            {
                Console.WriteLine("UpdatePassword Error: Could not retrieve ID Token. User needs to re-authenticate.");
                
                return false;
            }

            
            string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key=AIzaSyDnE-unnTD3dSGWzovUUUjYWvMI4NTR2DQ";

           
            var payload = new
            {
                idToken,
                password = newPassword, 
                returnSecureToken = true
            };

            
            var response = await _httpClient.PostAsJsonAsync(requestUri, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Firebase Password Update Error: {error}");
                return false;
            }

           
            Console.WriteLine("Password successfully updated.");

           
            return true;
        }
        //for change phone number
        public async Task<bool> UpdatePhoneNumberAsync(string newPhoneNumber)
        {
            string? idToken = await GetCurrentIDTokenAsync();
            if (idToken == null)
            {
                Console.WriteLine("UpdatePhoneNumber Error: Could not retrieve ID Token.");
                return false;
            }

            // Uses the _apiKey variable which holds "AIzaSyDnE-unnTD3dSGWzovUUUjYWvMI4NTR2DQ"
            string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={_apiKey}";

            var payload = new
            {
                idToken,
                phoneNumber = newPhoneNumber,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync(requestUri, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Firebase Phone Number Update Error: {error}");
                return false;
            }

            Console.WriteLine($"Phone number successfully updated to: {newPhoneNumber}.");
            return true;
        }

        public async Task<bool> EmailExistsInFirebaseAsync(string email)
        {
            string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:createAuthUri?key={_apiKey}";
            var payload = new { identifier = email, continueUri = "http://localhost" };

            var response = await _httpClient.PostAsJsonAsync(requestUri, payload);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            // If "registered" = true, the email exists in Firebase Authentication
            return result != null && result.TryGetValue("registered", out var registered) && (bool)registered;
        }
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            string requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={FirebaseApiKey}";
            var payload = new { requestType = "PASSWORD_RESET", email };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(requestUri, payload);
                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Firebase raw response: " + result);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Firebase error: {result}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                throw;
            }
        }
    }
}
