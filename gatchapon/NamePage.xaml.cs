using gatchapon.Models;
using Microsoft.Maui.Storage;

namespace gatchapon
{
    public partial class NamePage : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private string _userId;

        // Constructor accepts the UserId passed from RegisterPage
        public NamePage(string userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            string name = Name123.Text?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                await DisplayAlert("Oops", "Please enter a name to continue.", "OK");
                return;
            }

            try
            {
                // 1. Get the current user data
                var user = await _dbService.GetUserAsync<UserModel>(_userId);

                if (user != null)
                {
                    // 2. Update the Username locally
                    user.Username = name;

                    // 3. Save changes back to Firebase
                    await _dbService.SaveUserAsync(_userId, user);

                    // 4. Save to Local Storage for easy access
                    await SecureStorage.SetAsync("userId", _userId);
                    await SecureStorage.SetAsync("userName", name);

                    await DisplayAlert("Nice!", $"Welcome to the world, {name}!", "OK");

                    // 5. Navigate to Dashboard (Resetting the stack so they can't go back)
                    Application.Current.MainPage = new AppShell();
                    await Shell.Current.GoToAsync("//Dashboard");
                }
                else
                {
                    await DisplayAlert("Error", "User not found. Please restart.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not save name: {ex.Message}", "OK");
            }
        }
    }
}