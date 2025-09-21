
namespace gatchapon;

public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {

        if (EmailEntry.Text == "admin" && PasswordEntry.Text =="admin")
        {
           await Shell.Current.GoToAsync("ProfileSetting");
        }

        else
        {
            await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
        }

    }
}
