using Microsoft.Maui.Controls;
using System;

namespace gatchapon
{
    public partial class ForgotPass : ContentPage
    {
        private readonly FirebaseAuthService _authService = new FirebaseAuthService();

        public ForgotPass()
        {
            InitializeComponent();
        }

        private async void OnRPCBTN(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();

            if (string.IsNullOrEmpty(email))
            {
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = "Please enter your email.";
                return;
            }

            try
            {
                // Send reset email directly
                bool success = await _authService.SendPasswordResetEmailAsync(email);
                StatusLabel.TextColor = Colors.Green;
                StatusLabel.Text = "Reset link sent, Please check your email.";
            }
            
            catch (Exception)
                {
                    StatusLabel.TextColor = Colors.Red;
                    StatusLabel.Text = "Error Please Try again, if persist please try other email.";
                }
        }

        private async void BTL(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
