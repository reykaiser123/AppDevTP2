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
            var email = emailEntryForgot.Text?.Trim();

            if (string.IsNullOrEmpty(email))
            {
                await DisplayAlert("Error", "Please enter your email.", "OK");
                return;
            }

            try
            {
                bool success = await _authService.SendPasswordResetEmailAsync(email);

                if (success)
                    await DisplayAlert("Success", "Reset link sent. Check your email.", "OK");
                else
                    await DisplayAlert("Failed", "Failed to send reset email. Please try again.", "OK");
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "Something went wrong. Try again or use a different email.", "OK");
            }
        }

        private async void BTL(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
