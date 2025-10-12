using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
namespace gatchapon
{
    public partial class Login : ContentPage
    {
        bool isPasswordVisible = false;
        private readonly FirebaseAuthService _authService = new(); // <-- Create FirebaseAuthService instance

        public Login()
        {
            InitializeComponent();
        }
        private async void Logsbtn(object sender, EventArgs e)
        {

            string emaillogin = emailEntry.Text;
            string password = passwordEntry.Text;

            
            string email = $"{emaillogin}";

            var authService = new FirebaseAuthService();

            var signInResult = await _authService.SignInResponseAsync(email, password);

            if (signInResult != null)
            {
                var dbService = new FirebaseDatabaseService();
                var user = await dbService.GetUserAsync<dynamic>(signInResult.localId);

                await DisplayAlert("Success", "Login Successful", "OK");
                await Shell.Current.GoToAsync("//Dashboard");
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
            }



        }
        private async void Createhere(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Register");
        }
        private async void onForgotPassBTN(object sender, EventArgs e)
        {

            await Shell.Current.GoToAsync("ForgotPass");
        }


    }
}