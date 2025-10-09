using Microsoft.Maui.Controls;
using System;
namespace gatchapon
{
    public partial class Register : ContentPage
    {
        private readonly FirebaseAuthService _authService = new();

        public Register()
        {
            InitializeComponent();

        }
        private async void OnRegisterBTN(object sender, EventArgs e)
        {
            string email = emailEntryReg.Text?.Trim();
            string password = passwordEntryReg.Text;
            string confirm = confirmPass.Text;
                
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirm))
            {
                await DisplayAlert("Error", "Please enter all fields.", "OK");
                return;
            }

            if (password != confirm)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            var authService = new FirebaseAuthService();
            var result = await _authService.SignUpWithEmailPasswordAsync(email, password);

            if (result == null)
            {
                var dbService = new FirebaseDatabaseService();
                var user = new { Email = result.email, LocalId = result.localId, CreatedAt = DateTime.UtcNow };
                await dbService.SaveUserAsync(result.localId, user);

                await DisplayAlert("Error", "Registration failed. Check your email format or try again.", "OK");
                return;
            }

            else
            {
                await DisplayAlert("Success", "Account created successfully!", "OK");

                await Shell.Current.GoToAsync("//Login");

            }
        }
    }
}
