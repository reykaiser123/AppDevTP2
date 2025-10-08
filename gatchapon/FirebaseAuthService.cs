using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


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

        private const string FirebaseApiKey = "AIzaSyDnE-unnTD3dSGWzovUUUjYWvMI4NTR2DQ"; 

        public async Task<FirebaseSignInResponse?> SignInWithEmailPasswordAsync(string email, string password)
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
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsJsonAsync(requestUri, payload);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Firebase Sign-Up Error: {error}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during Firebase sign-up: {ex.Message}");
                return null;
            }
        }
    }
}