
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using gatchapon;

namespace gatchapon
{
    public partial class AppShell : Shell
    {
        // 🚨 CRITICAL FIX: You must declare and instantiate the service here
        // so that the CheckLoginStatusAndNavigate method can see it.
        private readonly FirebaseAuthService _authService = new FirebaseAuthService();

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(Register), typeof(Register));
            Routing.RegisterRoute(nameof(Login), typeof(Login));
            Routing.RegisterRoute(nameof(ForgotPass), typeof(ForgotPass));
            Routing.RegisterRoute(nameof(ProfileSetting), typeof(ProfileSetting));
            Routing.RegisterRoute(nameof(Dashboard), typeof(Dashboard));
            Routing.RegisterRoute(nameof(Shop), typeof(Shop));
            Routing.RegisterRoute(nameof(GachaBanner), typeof(GachaBanner));

            // Starts the asynchronous check when the app launches
            
        }
        private void OnShellLoaded(object sender, EventArgs e)
        {
            // Call the navigation logic ONLY when the Shell is confirmed to be loaded.
            _ = CheckLoginStatusAndNavigate();
        }
        private async Task CheckLoginStatusAndNavigate()
        {
            // Now '_authService' is recognized by the compiler
            bool isLoggedIn = await _authService.IsUserLoggedInAsync();

            if (isLoggedIn)
            {
                await Shell.Current.GoToAsync($"//{nameof(Dashboard)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(Login)}");
            }
        }
    }
}