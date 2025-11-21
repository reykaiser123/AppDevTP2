using gatchapon.Models;

namespace gatchapon
{
    public partial class News : ContentPage
    {
        public News()
        {
            InitializeComponent();
        }

        // Navigation Back Logic
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Sort Button Logic
        private async void OnSortClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Sort News By:", "Cancel", null, "Newest First", "Oldest First");
            if (action == "Newest First")
            {
                await DisplayAlert("Sort", "Sorted by Newest", "OK");
                // Add real sorting logic here later
            }
            else if (action == "Oldest First")
            {
                await DisplayAlert("Sort", "Sorted by Oldest", "OK");
            }
        }
    }
}