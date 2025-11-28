using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using gatchapon.Models;
using Microsoft.Maui.Storage; // Needed for SecureStorage

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

            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both email and password.", "OK");
                return;
            }

            // 2. Attempt Sign In
            var signInResult = await _authService.SignInResponseAsync(email, password);

            if (signInResult != null)
            {
                string userId = signInResult.localId;

                // CRITICAL: Save the ID so Dashboard can use it!
                await SecureStorage.SetAsync("userId", userId);

                // 3. Fetch User Profile (Self-Healing Logic)
                var userModel = await _dbService.GetUserAsync<UserModel>(userId);

                // If the account exists in Auth but has NO data in the Database (Realtime DB), fix it now:
                if (userModel == null)
                {
                    userModel = new UserModel
                    {
                        UserId = userId,
                        Email = email,
                        Username = "Traveler", // Default Name
                        Gold = 5000,           // Welcome Bonus
                        Gems = 0,
                        UnlockedCharacters = new List<string>(),
                        FriendsCount = 0
                    };

                    // Save this new/restored profile to Firebase
                    await _dbService.SaveUserAsync(userId, userModel);
                }

                // 4. Save Name locally for easy access
                string displayName = !string.IsNullOrEmpty(userModel.Username) ? userModel.Username : "Traveler";
                await SecureStorage.SetAsync("userName", displayName);

                await DisplayAlert("Welcome", $"Welcome back, {displayName}!", "OK");

                // 5. NAVIGATE TO DASHBOARD
                // Using '///DashboardPage' to match your AppShell Route
                await Shell.Current.GoToAsync("///DashboardPage");
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
            }
        }

        private async void Createhere(object sender, EventArgs e)
        {
            // Navigate to Register Page
            await Shell.Current.GoToAsync("Register");
        }

        private async void onForgotPassBTN(object sender, EventArgs e)
        {
            // Navigate to Forgot Password Page
            await Shell.Current.GoToAsync("ForgotPass");
        }
    }
}