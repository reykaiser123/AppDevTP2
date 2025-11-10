namespace gatchapon;

public partial class ResultPage : ContentPage
{
    private readonly List<GachaItem> items;
    private readonly Random random;
    private int pullsSinceEpic;
    private int totalPulls;
    private readonly Action<int> updatePityCallback;
    

    public ResultPage(List<GachaItem> results, List<GachaItem> itemPool, Random rng, int pityCount, Action<int> onUpdatePity)
    {
        InitializeComponent();

        ResultCollection.ItemsSource = results ?? new List<GachaItem>();

        UpdateUI();
        items = itemPool;
        random = rng;
        pullsSinceEpic = pityCount;
        totalPulls = pityCount;
        updatePityCallback = onUpdatePity;

        this.Appearing += (s, e) =>
        {
            if (ResultCollection != null)
                ResultCollection.ItemsSource = results;

            if (PullCountLabel != null)
                PullCountLabel.Text = $"Total pulls: {pullsSinceEpic} / 80 (resets after Epic)";
        };
    }
    private void UpdateUI()
    {
        goldcoin.Text = PlayerSession.Player.Coin.ToString();
    }

    private void SafeShowResults(List<GachaItem> results)
    {
        PullCountLabel.Text = $"Total pulls: {pullsSinceEpic} / 80 (resets after Epic)";
        ResultCollection.ItemsSource = results;
    }

    private void OnPullAgainClicked(object sender, EventArgs e)
    {
        if (PlayerSession.Player.Coin >= 10)
        {
            PlayerSession.Player.Coin -= 1;
            goldcoin.Text = PlayerSession.Player.Coin.ToString();
            var newResults = new List<GachaItem>();
            for (int i = 0; i < 10; i++)
                newResults.Add(PullItem());

            SafeShowResults(newResults);
            updatePityCallback?.Invoke(pullsSinceEpic);
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
        totalPulls++;

        if (pullsSinceEpic >= 80)
        {
            pullsSinceEpic = 0;
            totalPulls = 0;
            return items.First(i => i.Name == "Epic");
        }

        double roll = random.NextDouble();
        double cumulative = 0;

        foreach (var item in items)
        {
            cumulative += item.Rate;
            if (roll < cumulative)
            {
                if (item.Name == "Epic")
                {
                    pullsSinceEpic = 0;
                    totalPulls = 0;
                }
                updatePityCallback?.Invoke(pullsSinceEpic);
                return item;
            }
        }

        updatePityCallback?.Invoke(pullsSinceEpic);
        return items[0];
    }
}