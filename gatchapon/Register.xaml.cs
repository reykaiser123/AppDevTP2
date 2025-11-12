using Microsoft.Maui.Controls;
using System;
using gatchapon.Models; // 1. ADD THIS to get your UserModel

namespace gatchapon
{
    public partial class Register : ContentPage
    {
        private readonly FirebaseAuthService _authService = new();

        public Register()
        {
            InitializeComponent();
        }

        private async void OnRegisterBTN(object sender, EventArgs e)
        {
            string email = emailEntryReg.Text?.Trim();
            string password = passwordEntryReg.Text;
            string confirm = confirmPass.Text;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirm))
            {
                await DisplayAlert("Error", "Please enter all fields.", "OK");
                return;
            }

            if (password != confirm)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            // We already have _authService defined in the class, no need for a new one
            var result = await _authService.SignUpWithEmailPasswordAsync(email, password);

            if (result == null)
            {
                await DisplayAlert("Error", "Registration failed. Check your email format or try again.", "OK");
                return;
            }

            await _authService.SaveUserSessionAsync(result); // this saves refresh_token and user_email

            // --- 2. THIS IS THE UPDATED SECTION ---
            var dbService = new FirebaseDatabaseService();

            // Create a new user using your proper UserModel
            var newUser = new UserModel
            {
                UserId = result.localId,
                Email = result.email,
                // We set Username to blank, because the 'NamePage' will fill it in
                Username = "",

                // Set the default values for your game
                Gold = 0,
                Gems = 0,
                CheckInStreak = 0,
                LastCheckInDate = "",
                LastTaskCompletionDate = "",
                TasksCompletedToday = 0,
                MonthlyCheckIns = new List<string>(), // Initialize the list
                Claimed3Tasks = false,
                Claimed7DayStreak = false,
                ClaimedMonthly = false
            };

            // Save the new, complete user object to Firebase
            await dbService.SaveUserAsync(result.localId, newUser);
            // --- END OF UPDATED SECTION ---

            //Store UserId securely on device
            await SecureStorage.SetAsync("userId", result.localId);

            await DisplayAlert("Success", "Account created successfully!", "OK");

            // Navigate to the NamePage to ask for their username
            await Shell.Current.GoToAsync($"{nameof(NamePage)}?userId={result.localId}");
        }

        private async void Clkhere(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}