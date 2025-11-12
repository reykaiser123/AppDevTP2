using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using gatchapon.Models;

namespace gatchapon
{
    public partial class ProfileSetting : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new();

        public ProfileSetting()
        {
            InitializeComponent();
            // We call LoadUserProfile in OnAppearing, so it's
            // not needed here. This prevents loading twice on startup.
        }

        // Override OnAppearing to refresh data when the page is navigated to
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserProfile();  // Reload profile each time the page appears
        }

        // --- THIS IS THE UPDATED METHOD ---
        private async void LoadUserProfile()
        {
            string userId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(userId))
            {
                Userlabel.Text = "No user logged in";
                EmailDisplayLabel.Text = "No email found"; // Set email label too
                return;
            }

            // We use UserModel, which is now consistent
            // thanks to our [JsonPropertyName] fixes.
            var user = await _dbService.GetUserAsync<UserModel>(userId);

            if (user != null)
            {
                // Set Username
                if (!string.IsNullOrEmpty(user.Username))
                {
                    Userlabel.Text = user.Username;
                }
                else
                {
                    Userlabel.Text = "No name set";
                }

                // --- I ADDED THIS PART ---
                // Set Email
                if (!string.IsNullOrEmpty(user.Email))
                {
                    EmailDisplayLabel.Text = user.Email;
                }
                else
                {
                    EmailDisplayLabel.Text = "No email set";
                }
            }
            else
            {
                Userlabel.Text = "User not found";
                EmailDisplayLabel.Text = "User not found";
            }
        }
        // --- END OF UPDATED METHOD ---

        public async void OnLogout(object? sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Log out", "Are you sure you want to log out?", "Yes", "No");
            if (confirm)
            {
                try
                {
                    await _authService.LogOut();
                    // Clear the stored userId to prevent stale data on next login
                    SecureStorage.Remove("userId");
                    SecureStorage.Remove("userName");

                    await DisplayAlert("Logged Out", "You have been logged out successfully.", "OK");
                    await Shell.Current.GoToAsync("//Login");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Logout failed: {ex.Message}", "OK");
                }
            }
        }

        //profile image clicked
        private async void OnProf(object sender, EventArgs e)
        {
            await DisplayAlert("Profile Image", "Profile image clicked!", "OK");
        }

        //change name button clicked
        private async void Cnamebtn(object sender, EventArgs e)
        {
            // You can navigate to your NamePage here if you want
            // await Shell.Current.GoToAsync(nameof(NamePage));
            await DisplayAlert("Not Implemented", "Change Name page not built yet.", "OK");
        }

        //change phone button clicked
        private async void Cphonebtn(object sender, EventArgs e)
        {
            await DisplayAlert("Not Implemented", "Change Phone page not built yet.", "OK");
        }

        // Note: I changed 'EventArgs' to 'TappedEventArgs' to match the XAML
        private async void OnAppV(object sender, TappedEventArgs e)
        {
            await DisplayAlert("App Version", "v0.1", "OK");
        }

        // Note: I changed 'EventArgs' to 'TappedEventArgs' to match the XAML
        private async void OnSup(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Support", "Support page not built yet.", "OK");
        }
    }
}