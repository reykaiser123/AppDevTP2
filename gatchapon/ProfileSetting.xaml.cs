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
            LoadUserProfile();
        }

        // Override OnAppearing to refresh data when the page is navigated to (e.g., after login)
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserProfile();  // Reload profile each time the page appears
        }

        private async void LoadUserProfile()
        {
            string userId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(userId))
            {
                Userlabel.Text = "No user logged in";
                return;
            }

            var user = await _dbService.GetUserAsync<UserModel>(userId);

            if (user != null && !string.IsNullOrEmpty(user.Username))
            {
                Userlabel.Text = user.Username;
            }
            else
            {
                Userlabel.Text = "No name set";
            }
        }

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
    }
}
