using gatchapon.Models;

namespace gatchapon
{
    public partial class Shop : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private string _currentUserId;
        private UserModel _currentUser;

        public Shop()
        {
            InitializeComponent();
        }
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _currentUserId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(_currentUserId))
            {
                // If this happens, your login session is lost
                await DisplayAlert("Error", "No User ID found. Please log in again.", "OK");
                return;
            }

            await LoadUserData();
        }

        private async Task LoadUserData()
        {
            try
            {
                // 1. Fetch User from Firebase
                _currentUser = await _dbService.GetUserAsync<UserModel>(_currentUserId);

                // 2. Update UI
                if (_currentUser != null)
                {
                    // Force update on Main Thread to ensure TitleView updates correctly
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        GoldLabel.Text = _currentUser.Gold.ToString();
                    });
                }
                else
                {
                    // Debugging: If this shows, your User ID exists but DB data is empty
                    Console.WriteLine("User data not found in database for this ID.");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load data: {ex.Message}", "OK");
            }
        }

        // --- BUY STREAM GEAR CRATE (1000 GOLD) ---
        private async void OnBuyCrateClicked(object sender, TappedEventArgs e)
        {
            if (_currentUser == null) return;

            if (_currentUser.HasStreamGearCrate)
            {
                await DisplayAlert("Owned", "You already own this item!", "OK");
                return;
            }

            int itemCost = 1000;
            await ProcessPurchase(itemCost, "Stream Gear Crate", () => _currentUser.HasStreamGearCrate = true);
        }

        // --- BUY SKY-HIGH SCARF (300 GOLD) ---
        private async void OnBuyScarfClicked(object sender, TappedEventArgs e)
        {
            if (_currentUser == null) return;

            if (_currentUser.HasSkyHighScarf)
            {
                await DisplayAlert("Owned", "You already own this item!", "OK");
                return;
            }

            int itemCost = 300;
            await ProcessPurchase(itemCost, "Sky-High Scarf", () => _currentUser.HasSkyHighScarf = true);
        }

        // --- BUY WOVEN CLOUD TAPESTRY (2000 GOLD) ---
        private async void OnBuyTapestryClicked(object sender, TappedEventArgs e)
        {
            if (_currentUser == null) return;

            if (_currentUser.HasWovenCloudTapestry)
            {
                await DisplayAlert("Owned", "You already own this item!", "OK");
                return;
            }

            int itemCost = 2000;
            await ProcessPurchase(itemCost, "Woven Cloud Tapestry", () => _currentUser.HasWovenCloudTapestry = true);
        }

        // --- HELPER FUNCTION TO AVOID REPEATING CODE ---
        private async Task ProcessPurchase(int cost, string itemName, Action setOwned)
        {
            if (_currentUser.Gold >= cost)
            {
                bool confirm = await DisplayAlert("Confirm", $"Buy '{itemName}' for {cost} gold?", "Yes", "No");
                if (confirm)
                {
                    _currentUser.Gold -= cost;
                    setOwned(); // Execute the action to set the specific item to true

                    await _dbService.SaveUserAsync(_currentUserId, _currentUser);

                    // Force UI Update
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        GoldLabel.Text = _currentUser.Gold.ToString();
                    });

                    await DisplayAlert("Success", "Item added to Inventory!", "OK");
                }
            }
            else
            {
                await DisplayAlert("Not Enough Gold", "You need more gold.", "OK");
            }
        }
    }
}