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

}