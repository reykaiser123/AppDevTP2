using Microsoft.Maui.Controls;
using System;
namespace gatchapon
{
    public partial class Login : ContentPage
    {


        public Login()
        {
            InitializeComponent();
        }
        private void Signin(object sender, EventArgs e)
        {
            String username = emailEntry.Text;
            String password = passwordEntry.Text;
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }
            if (username == "admin" && password == "1234")
                DisplayAlert("Success", "Login Successful", "OK");
            else
                DisplayAlert("Error", "Invalid username or password", "OK");
        }
        private void ForgotPass(object sender, EventArgs e)
        {
            DisplayAlert("Info", "Forgot password clicked", "OK");
        }



    }
}
