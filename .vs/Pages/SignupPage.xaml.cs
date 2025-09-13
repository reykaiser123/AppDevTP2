namespace projectpt2.Pages;

public partial class SignupPage : ContentPage
{
	public SignupPage()
	{
		InitializeComponent();
	}

    private void loginbtn_Clicked(object sender, EventArgs e)
    {
		Navigation.PushModalAsync(new LoginPage());
    }
}