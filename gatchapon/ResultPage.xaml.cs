using gatchapon.Models;

namespace gatchapon;

public partial class ResultPage : ContentPage
{
    private readonly FirebaseDatabaseService _dbService = new();
    private string _currentUserId;
    private UserModel _currentUser;

    private readonly List<GachaItem> items;
    private readonly Random random;
    private int pullsSinceEpic;
    private readonly Action<int> updatePityCallback;

    public ResultPage(List<GachaItem> results, List<GachaItem> itemPool, Random rng, int pityCount, Action<int> onUpdatePity)
    {
        InitializeComponent();

        ResultCollection.ItemsSource = results ?? new List<GachaItem>();

        items = itemPool;
        random = rng;
        pullsSinceEpic = pityCount;
        updatePityCallback = onUpdatePity;
    }

    // Load data when page appears so we know how much gold we have
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _currentUserId = await SecureStorage.GetAsync("userId");

        if (!string.IsNullOrEmpty(_currentUserId))
        {
            await LoadUserData();
        }
    }

    private async Task LoadUserData()
    {
        _currentUser = await _dbService.GetUserAsync<UserModel>(_currentUserId);
        if (_currentUser != null)
        {
            goldcoin.Text = _currentUser.Gold.ToString();
        }
        PullCountLabel.Text = $"Pity: {pullsSinceEpic} / 80";
    }

    private async void OnPullAgainClicked(object sender, EventArgs e)
    {
        // 10x Pull Cost
        int cost = 1000;

        if (_currentUser == null) return;

        if (_currentUser.Gold >= cost)
        {
            // 1. Deduct Gold
            _currentUser.Gold -= cost;
            goldcoin.Text = _currentUser.Gold.ToString(); // UI Update

            // 2. Logic for 10 Pulls
            var newResults = new List<GachaItem>();
            for (int i = 0; i < 10; i++)
            {
                var item = PullItem();
                newResults.Add(item);

                // Save Character if new
                if (!_currentUser.UnlockedCharacters.Contains(item.Name))
                {
                    _currentUser.UnlockedCharacters.Add(item.Name);
                }
            }

            // 3. Save everything to Firebase
            await _dbService.SaveUserAsync(_currentUserId, _currentUser);

            // 4. Refresh UI List
            ResultCollection.ItemsSource = null;
            ResultCollection.ItemsSource = newResults;

            updatePityCallback?.Invoke(pullsSinceEpic);
            PullCountLabel.Text = $"Pity: {pullsSinceEpic} / 80";
        }
        else
        {
            await DisplayAlert("No Gold", "Not enough gold to pull again.", "OK");
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        updatePityCallback?.Invoke(pullsSinceEpic);
        await Navigation.PopModalAsync();
    }

    private GachaItem PullItem()
    {
        pullsSinceEpic++;

        if (pullsSinceEpic >= 80)
        {
            pullsSinceEpic = 0;
            return items.First(i => i.Rarity == "Epic");
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
}