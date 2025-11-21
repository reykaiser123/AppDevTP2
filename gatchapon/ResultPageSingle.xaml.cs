using gatchapon.Models;

namespace gatchapon;

public partial class ResultPageSingle : ContentPage
{
    private readonly FirebaseDatabaseService _dbService = new();
    private string _currentUserId;
    private UserModel _currentUser;

    private readonly List<GachaItem> items;
    private readonly Random random;
    private int pullsSinceEpic;
    private Action<int> updatePityCallback;

    public ResultPageSingle(GachaItem result, List<GachaItem> itemPool, Random rng, int pityCount, Action<int> onUpdatePity)
    {
        InitializeComponent();

        items = itemPool;
        random = rng;
        pullsSinceEpic = pityCount;
        updatePityCallback = onUpdatePity;

        // Show the result passed from the banner immediately
        ShowResult(result);
    }

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

    private void ShowResult(GachaItem item)
    {
        PullCountLabel.Text = $"Pity: {pullsSinceEpic} / 80";
        ResultLabel.Text = item.Name;
        ResultImage.Source = item.Image;
    }

    private async void OnPullAgainClicked(object sender, EventArgs e)
    {
        // 1x Pull Cost
        int cost = 100;

        if (_currentUser == null) return;

        if (_currentUser.Gold >= cost)
        {
            // 1. Deduct Gold
            _currentUser.Gold -= cost;
            goldcoin.Text = _currentUser.Gold.ToString();

            // 2. Pull Logic
            var newItem = PullItem();

            // Save if new character
            if (!_currentUser.UnlockedCharacters.Contains(newItem.Name))
            {
                _currentUser.UnlockedCharacters.Add(newItem.Name);
            }

            // 3. Save to Firebase
            await _dbService.SaveUserAsync(_currentUserId, _currentUser);

            // 4. Show Result
            ShowResult(newItem);

            updatePityCallback?.Invoke(pullsSinceEpic);
        }
        else
        {
            await DisplayAlert("No Gold", "Not enough gold.", "OK");
        }
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

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        updatePityCallback?.Invoke(pullsSinceEpic);
        await Navigation.PopModalAsync();
    }
}