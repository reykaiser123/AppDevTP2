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
                await Shell.Current.GoToAsync("Dashboard");

                Application.Current.MainPage = new NavigationPage(new Dashboard());
            }

            else
            {
                DisplayAlert("Error", "Invalid username or password", "OK");
            }
                
        }
        private async void Forgot(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///ForgotPass");
        }
        private async void Createhere(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Register");
        }


    }
}