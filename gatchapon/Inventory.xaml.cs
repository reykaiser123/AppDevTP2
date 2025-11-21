using gatchapon.Models;

namespace gatchapon
{
    public partial class Inventory : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private string _currentUserId;

        public Inventory()
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

            if (!string.IsNullOrEmpty(_currentUserId))
            {
                await LoadInventory();
            }
        }

        private async Task LoadInventory()
        {
            try
            {
                var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                if (user != null)
                {
                    // Show items ONLY if the user owns them (True)
                    CrateItem.IsVisible = user.HasStreamGearCrate;
                    ScarfItem.IsVisible = user.HasSkyHighScarf;
                    TapestryItem.IsVisible = user.HasWovenCloudTapestry;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Could not load inventory.", "OK");
            }
        }

        // --- DESCRIPTION POPUPS ---

        private async void OnCrateClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Stream Gear Crate",
                "A high-tech crate containing specialized streaming equipment for Marisol. Increases tech skill by 15%.",
                "Close");
        }

        private async void OnScarfClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Sky-High Scarf",
                "A magical scarf woven from the clouds themselves. Grants the wearer a feeling of lightness.",
                "Close");
        }

        private async void OnTapestryClicked(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Woven Cloud Tapestry",
                "A legendary artifact depicting the ancient skies. It is said to bring good fortune to those who gaze upon it.",
                "Close");
        }
    }
}