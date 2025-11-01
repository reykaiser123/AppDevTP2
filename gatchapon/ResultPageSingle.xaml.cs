namespace gatchapon;

public partial class ResultPageSingle : ContentPage
{
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

        ShowResult(result);
    }

    private void ShowResult(GachaItem item)
    {
        PullCountLabel.Text = $"Total pulls: {pullsSinceEpic} / 80 (resets after Epic)";
        ResultLabel.Text = item.Name;
        ResultImage.Source = item.Image;
    }

    private GachaItem PullItem()
    {
        pullsSinceEpic++;

        if (pullsSinceEpic >= 80)
        {
            pullsSinceEpic = 0;
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
                    pullsSinceEpic = 0;
                return item;
            }
        }

        return items[0];
    }

    private void OnPullAgainClicked(object sender, EventArgs e)
    {
        var newItem = PullItem();
        ShowResult(newItem);
        updatePityCallback?.Invoke(pullsSinceEpic);
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        updatePityCallback?.Invoke(pullsSinceEpic);
        await Shell.Current.GoToAsync("//Dashboard/GachaBanner");
    }

}