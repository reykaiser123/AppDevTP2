namespace projectpt2.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		
	}
	private void signupbtn_Clicked(object sender, EventArgs e)
	{
		Navigation.PushModalAsync(new SignupPage());

	}
}