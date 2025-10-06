using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
namespace gatchapon
{
    public partial class Login : ContentPage
    {

        public Login()
        {
            InitializeComponent();
        }
        private async void Logsbtn(object sender, EventArgs e)
        {
            String username = emailEntry.Text;
            String password = passwordEntry.Text;
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }
            if (username == "admin" && password == "1234")
            {
                DisplayAlert("Success", "Login Successful", "OK");
                await Shell.Current.GoToAsync("//Dashboard");

                //Application.Current.MainPage = new NavigationPage(new Dashboard());
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

            else
            {
                DisplayAlert("Error", "Invalid username or password", "OK");
            }
                
        }
        private async void Createhere(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Register");
        }
        private async void onForgotPassBTN(object sender, EventArgs e)
        {
            
            await Shell.Current.GoToAsync("/ForgotPass");
        }


    }
}