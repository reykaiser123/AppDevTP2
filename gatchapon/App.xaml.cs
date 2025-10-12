using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace gatchapon
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            _ = CheckLoginStateAsync();
            return window;

        }
        private async Task CheckLoginStateAsync()
        {
            try
            {
                while (Shell.Current == null)
                    await Task.Delay(200);

                var refreshToken = await SecureStorage.GetAsync("refresh_token");

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var authService = new FirebaseAuthService();
                    var newToken = await authService.RefreshIdTokenAsync(refreshToken);

                    if (newToken != null)
                    {
                        await Shell.Current.GoToAsync("//Dashboard");
                        return;
                    }
                }

                await Shell.Current.GoToAsync("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking login state: {ex.Message}");
                await Shell.Current.GoToAsync("//Login");
            }

        }
        
    }
}