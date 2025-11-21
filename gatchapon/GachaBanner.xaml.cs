using gatchapon.Models;

namespace gatchapon
{
    public partial class GachaBanner : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private string _currentUserId;
        private UserModel _currentUser;

        private readonly List<GachaItem> items;
        private readonly Random random = new Random();
        private int pullsSinceEpic = 0;

        private const int SinglePullCost = 100;
        private const int TenPullCost = 1000;

        public GachaBanner()
        {
            InitializeComponent();

            // UPDATED: Using the real filenames from your screenshot
            items = new List<GachaItem>
            {
                // Common - Roberto
                // Uses "roberto_char.png" from your file list
                new GachaItem("Roberto", "roberto_char.png", 0.7, "Common"), 
                
                // Rare - Maxine
                // Uses "maxine_char.png" from your file list
                new GachaItem("Maxine", "maxine_char.png", 0.25, "Rare"), 
                
                // Epic - Marisol
                // Uses "marisol_char.png" from your file list
                new GachaItem("Marisol", "marisol_char.png", 0.05, "Epic")
            };
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
                await DisplayAlert("Error", "Please log in to pull gacha.", "OK");
                return;
            }

            await LoadUserData();
        }

        private async Task LoadUserData()
        {
            try
            {
                _currentUser = await _dbService.GetUserAsync<UserModel>(_currentUserId);

                if (_currentUser != null)
                {
                    if (_currentUser.UnlockedCharacters == null)
                        _currentUser.UnlockedCharacters = new List<string>();

                    goldcoin.Text = _currentUser.Gold.ToString();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Could not load user data.", "OK");
            }
        }

        // --- 1x PULL BUTTON ---
        private async void OnSinglePullClicked(object sender, EventArgs e)
        {
            if (_currentUser == null) return;

            if (_currentUser.Gold >= SinglePullCost)
            {
                // 1. Deduct Gold
                _currentUser.Gold -= SinglePullCost;

                // 2. Pull Item
                var pulledItem = PullItem();

                // 3. SAVE CHARACTER TO DATABASE
                if (!_currentUser.UnlockedCharacters.Contains(pulledItem.Name))
                {
                    _currentUser.UnlockedCharacters.Add(pulledItem.Name);
                }

                // 4. Save to Firebase
                await _dbService.SaveUserAsync(_currentUserId, _currentUser);
                goldcoin.Text = _currentUser.Gold.ToString();

                // 5. Show Result (The image will now be correct!)
                await Navigation.PushModalAsync(
                    new ResultPageSingle(pulledItem, items, random, pullsSinceEpic, UpdatePullsSinceEpic)
                );
            }
            else
            {
                await DisplayAlert("Not Enough Gold", $"You need {SinglePullCost} gold to pull.", "OK");
            }
        }

        // --- 10x PULL BUTTON ---
        private async void OnTenPullClicked(object sender, EventArgs e)
        {
            if (_currentUser == null) return;

            if (_currentUser.Gold >= TenPullCost)
            {
                _currentUser.Gold -= TenPullCost;
                var results = new List<GachaItem>();

                for (int i = 0; i < 10; i++)
                {
                    var item = PullItem();
                    results.Add(item);

                    if (!_currentUser.UnlockedCharacters.Contains(item.Name))
                    {
                        _currentUser.UnlockedCharacters.Add(item.Name);
                    }
                }

                await _dbService.SaveUserAsync(_currentUserId, _currentUser);
                goldcoin.Text = _currentUser.Gold.ToString();

                await Navigation.PushModalAsync(
                    new ResultPage(results, items, random, pullsSinceEpic, UpdatePullsSinceEpic)
                );
            }
            else
            {
                await DisplayAlert("Not Enough Gold", $"You need {TenPullCost} gold to pull.", "OK");
            }
        }

        private GachaItem PullItem()
        {
            pullsSinceEpic++;

            if (pullsSinceEpic >= 80)
            {
                pullsSinceEpic = 0;
                return items.FirstOrDefault(i => i.Rarity == "Epic") ?? items[0];
            }

            double roll = random.NextDouble();
            double cumulative = 0;

            foreach (var item in items)
            {
                cumulative += item.Rate;
                if (roll < cumulative)
                {
                    if (item.Rarity == "Epic")
                        pullsSinceEpic = 0;
                    return item;
                }
            }

            return items[0];
        }

        private void UpdatePullsSinceEpic(int updatedCount)
        {
            pullsSinceEpic = updatedCount;
        }
    }

    public class GachaItem
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public double Rate { get; set; }
        public string Rarity { get; set; }

        public GachaItem(string name, string image, double rate, string rarity)
        {
            Name = name;
            Image = image;
            Rate = rate;
            Rarity = rarity;
        }
    }
}