using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace gatchapon
{
    [QueryProperty(nameof(UserId), "userId")]
    public partial class NamePage : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        public string UserId { get; set; }

        public NamePage()
        {
            InitializeComponent();
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            // This line assumes your Entry is named "Name123"
            string name = Name123.Text?.Trim();


            if (string.IsNullOrEmpty(name))
            {
                await DisplayAlert("Oops", "Please enter your name.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(UserId))
            {
                await DisplayAlert("Error", "User ID not found.", "OK");
                return;
            }

            // --- THIS IS THE FIX ---
            // Save name in Firebase using the lowercase "username" key
            await _dbService.UpdateUserFieldAsync(UserId, "username", name);
            // --- END OF FIX ---

            // Optional local storage
            // This line was already correct
            await SecureStorage.SetAsync("userId", UserId);
            await SecureStorage.SetAsync("userName", name);

            await DisplayAlert("Nice!", $"Welcome, {name}!", "OK");

            // Navigate to dashboard
            await Shell.Current.GoToAsync("//Dashboard");
        }
    }
}