namespace gatchapon;
public partial class GachaBanner : ContentPage
{
    private readonly List<GachaItem> items;
    private readonly Random random = new Random();
    private int pullsSinceEpic = 0;

    public GachaBanner()
    {
        InitializeComponent();

        items = new List<GachaItem>
            {
                new GachaItem("Common","dio.jpg", 0.7),
                new GachaItem("Rare","saba.jpg", 0.25),
                new GachaItem("Epic","ayame.jpg", 0.05)
            };
    }

            private async void OnSinglePullClicked(object sender, EventArgs e)
            {
                var pulledItem = PullItem();
                await Navigation.PushModalAsync(
                    new ResultPageSingle(pulledItem, items, random, pullsSinceEpic, UpdatePullsSinceEpic)
                );
            }

            // --- TEN PULL ---
            private async void OnTenPullClicked(object sender, EventArgs e)
            {
                var results = new List<GachaItem>();

                for (int i = 0; i < 10; i++)
                    results.Add(PullItem());

                await Navigation.PushModalAsync(new ResultPage(results, items, random, pullsSinceEpic, UpdatePullsSinceEpic));
            }

            private GachaItem PullItem()
            {
                pullsSinceEpic++;

                if (pullsSinceEpic >= 80)
                {
                    pullsSinceEpic = 0;
                    return items.FirstOrDefault(i => i.Name == "Epic") ?? items[0];
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

            // --- Callback to update pity from ResultPage ---
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

            public GachaItem(string name, string image, double rate)
            {
                Name = name;
                Image = image;
                Rate = rate;
    }
}
