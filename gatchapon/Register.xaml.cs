using gatchapon.Models;

namespace gatchapon
{
    public partial class Register : ContentPage
    {
        private readonly FirebaseAuthService _authService = new();
        private readonly FirebaseDatabaseService _dbService = new();

        public Register()
        {
            InitializeComponent();
        }

        private async void OnRegisterBTN(object sender, EventArgs e)
        {
            string email = emailEntryReg.Text?.Trim();
            string password = passwordEntryReg.Text;
            string confirm = confirmPass.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (password != confirm)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            try
            {
                // --- UPDATED SECTION FOR YOUR SERVICE ---

                // 1. Call your custom HTTP method
                var authResponse = await _authService.SignUpWithEmailPasswordAsync(email, password);

                // 2. Check if it was successful
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.localId))
                {
                    string userId = authResponse.localId;

                    // 3. Create User Model (5000 Gold Bonus)
                    var newUser = new UserModel
                    {
                        UserId = userId,
                        Email = email,
                        Username = "Traveler",
                        Gold = 5000, // 50 Pulls
                        Gems = 0,
                        UnlockedCharacters = new List<string>(),
                        HasStreamGearCrate = false,
                        HasSkyHighScarf = false,
                        HasWovenCloudTapestry = false
                    };

                    // 4. Save to Database
                    await _dbService.SaveUserAsync(userId, newUser);

                    // 5. Go to NamePage
                    await Navigation.PushAsync(new NamePage(userId));
                }
                else
                {
                    await DisplayAlert("Error", "Registration failed. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Registration Failed", ex.Message, "OK");
            }
        }

        private async void Clkhere(object sender, TappedEventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}