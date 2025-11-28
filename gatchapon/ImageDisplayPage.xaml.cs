namespace gatchapon
{
    [QueryProperty(nameof(ImageUrl), "imageUrl")]
    public partial class ImageDisplayPage : ContentPage
    {
        public string ImageUrl { get; set; }

        public ImageDisplayPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!string.IsNullOrEmpty(ImageUrl))
            {
                string decodedUrl = Uri.UnescapeDataString(ImageUrl);

                // Check if it's a local file path (for device uploads)
                if (decodedUrl.Contains("/") || decodedUrl.Contains("\\"))
                {
                    FullScreenImage.Source = ImageSource.FromFile(decodedUrl);
                }
                // Must be a resource name (character avatar)
                else
                {
                    FullScreenImage.Source = decodedUrl;
                }
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}