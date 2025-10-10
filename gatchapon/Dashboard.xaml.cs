namespace gatchapon;

public partial class Dashboard : ContentPage
{
	public Dashboard()
	{
		InitializeComponent();
		NavigationPage.SetHasBackButton(this, false);
    }
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfileSetting()); 
    }
    private async void Charbtn(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Characters());
    }
    private async void OnCharsImgClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GachaBanner());
    }
    private async void Shopbtn(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Shop());
    }

}