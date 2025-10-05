using Microsoft.Maui.Controls;
using System;
namespace gatchapon
{
    public partial class Register : ContentPage
    {

        public Register()
        {
            InitializeComponent();
        }
        private async void Regsbtn(object sender, EventArgs e)
        {
            String username = emailEntryReg.Text;
            String password = passwordEntryReg.Text;
            String confirm = confirmPass.Text;
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password)
                || String.IsNullOrWhiteSpace(confirm))
            {
                await DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }

            if (password != confirm)
            {
                await DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            if (username == "admin" && password == "1234")
                await DisplayAlert("Success", "Login Successful", "OK");
            else
                await DisplayAlert("Error", "Invalid username or password", "OK");
        }
     private async void Regshere(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Login");
        }


    }
}
