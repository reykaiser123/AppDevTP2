using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using gatchapon.Models; // <-- 1. MAKE SURE THIS IS ADDED

namespace gatchapon
{
    public partial class Login : ContentPage
    {
        bool isPasswordVisible = false;
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new();

        public Login()
        {
            InitializeComponent();
        }

        private async void Logsbtn(object sender, EventArgs e)
        {
            string email = emailEntry.Text;
            string password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both email and password.", "OK");
                return;
            }

            var signInResult = await _authService.SignInResponseAsync(email, password);

            if (signInResult != null)
            {
                //save userID locally
                string userId = signInResult.localId;
                await SecureStorage.SetAsync("userId", userId);

                // --- 2. THIS IS THE FIXED SECTION ---
                // Get user data from Firebase as a UserModel

                // --- THIS IS THE NEW, FIXED CODE ---
                var userData = await _dbService.GetUserAsync<Dictionary<string, object>>(userId);
                string username = null; // Start with no username

                if (userData != null)
                {
                    // Try to get the Capitalized version first
                    if (userData.ContainsKey("Username"))
                    {
                        username = userData["Username"].ToString();
                    }
                    // If that fails, try to get the lowercase version
                    else if (userData.ContainsKey("username"))
                    {
                        username = userData["username"].ToString();
                    }
                }

                // Now, check if we successfully found a username
                if (!string.IsNullOrEmpty(username))
                {
                    await SecureStorage.SetAsync("userName", username);
                    await DisplayAlert("Welcome Back", $"Welcome back, {username}!", "OK");
                }
                else
                {
                    await DisplayAlert("Login", "No username found for this account.", "OK");
                }
                // --- END OF NEW CODE ---
                // --- END OF FIXED SECTION ---

                await Shell.Current.GoToAsync("//Dashboard");
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
            }
        }

        private async void Createhere(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Register");
        }

        private async void onForgotPassBTN(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ForgotPass");
        }
    }
}