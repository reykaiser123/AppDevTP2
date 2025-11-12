using gatchapon.Models; // To find UserModel

namespace gatchapon
{
    public partial class Shop : ContentPage
    {
        // Get your services for auth and database
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new();

        private string _currentUserId;
        private UserModel _currentUser; // We'll store the user's data here

        public Shop()
        {
            InitializeComponent();
        }

        // This runs every time you open the shop page
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Get the User ID, just like we do in the Dashboard
            _currentUserId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(_currentUserId))
            {
                await DisplayAlert("Error", "You must be logged in to see the shop.", "OK");
                return;
            }

            // Load the user's gold and gems
            await LoadUserData();
        }

        private async Task LoadUserData()
        {
            try
            {
                // Get the user's data from Firebase
                _currentUser = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                if (_currentUser != null)
                {
                    // Update the "9999999" labels with the REAL data
                    GoldLabel.Text = _currentUser.Gold.ToString();
                    GemLabel.Text = _currentUser.Gems.ToString();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load user data: {ex.Message}", "OK");
            }
        }

        // This runs when you tap "Streak Saver"
        private async void OnBuyStreakSaverClicked(object sender, TappedEventArgs e)
        {
            int itemCost = 1000; // The price of the item
            if (_currentUser == null) return;

            // Check if the user has enough gold
            if (_currentUser.Gold >= itemCost)
            {
                bool confirm = await DisplayAlert("Confirm Purchase",
                    $"Buy 'Streak Saver' for {itemCost} gold?", "Yes", "No");

                if (confirm)
                {
                    // 1. Subtract the gold
                    _currentUser.Gold -= itemCost;

                    // 2. Save the updated user back to Firebase
                    await _dbService.SaveUserAsync(_currentUserId, _currentUser);

                    // 3. Update the UI to show the new gold amount
                    GoldLabel.Text = _currentUser.Gold.ToString();

                    await DisplayAlert("Success", "You bought a Streak Saver!", "OK");

                    // (Future code would add the item to your inventory here)
                }
            }
            else
            {
                // Not enough gold
                await DisplayAlert("Not Enough Gold",
                    $"You need {itemCost} gold, but you only have {_currentUser.Gold}.", "OK");
            }
        }

        // This runs when you tap "2x Boost"
        private async void OnBuyBoostClicked(object sender, TappedEventArgs e)
        {
            int itemCost = 50; // This item costs GEMS
            if (_currentUser == null) return;

            // Check for GEMS, not gold
            if (_currentUser.Gems >= itemCost)
            {
                bool confirm = await DisplayAlert("Confirm Purchase",
                    $"Buy '2x Boost (3day)' for {itemCost} gems?", "Yes", "No");

                if (confirm)
                {
                    // 1. Subtract the gems
                    _currentUser.Gems -= itemCost;

                    // 2. Save the updated user back to Firebase
                    await _dbService.SaveUserAsync(_currentUserId, _currentUser);

                    // 3. Update the UI
                    GemLabel.Text = _currentUser.Gems.ToString();

                    await DisplayAlert("Success", "You bought a 2x Boost!", "OK");
                }
            }
            else
            {
                await DisplayAlert("Not Enough Gems",
                    $"You need {itemCost} gems, but you only have {_currentUser.Gems}.", "OK");
            }
        }
    }
}