using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
namespace gatchapon
{
    public partial class Login : ContentPage
    {
        bool isPasswordVisible = false;
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new(); 

        public Login()
        {
            InitializeComponent();
        }
        private async void Logsbtn(object sender, EventArgs e)
        {

            string email = emailEntry.Text;
            string password = passwordEntry.Text;

            

            var authService = new FirebaseAuthService();
           

            if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both email and password.", "OK");
                                return;
            }

            var signInResult = await _authService.SignInResponseAsync(email, password);

            if (signInResult != null)
            {
                //save userID locally
                string userId = signInResult.localId;
                await SecureStorage.SetAsync("userId", userId);

                // Get user data form Firebase 
                var userData = await _dbService.GetUserAsync<Dictionary<string, object>>(userId);
                
                if(userData != null && userData.ContainsKey("Username"))
                {
                    string username = userData["Username"].ToString() ?? "User";
                    await SecureStorage.SetAsync("userName", username);
                    await DisplayAlert("Welcome Back", $"Welcome back, {username}!", "OK");
                }
                else
                {
                    await DisplayAlert("Login", "No username found for this account.", "OK");
                }
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