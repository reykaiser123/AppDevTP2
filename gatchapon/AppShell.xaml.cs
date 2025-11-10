
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
            Routing.RegisterRoute(nameof(Quest), typeof(Quest));
            Routing.RegisterRoute(nameof(News), typeof(News));
            Routing.RegisterRoute(nameof(Characters), typeof(Characters));
            Routing.RegisterRoute(nameof(ResultPage), typeof(ResultPage));
            Routing.RegisterRoute(nameof(ResultPageSingle), typeof(ResultPageSingle));
            Routing.RegisterRoute(nameof(NamePage), typeof(NamePage));


            

        }
        private void OnShellLoaded(object sender, EventArgs e)
        {
            
            _ = CheckLoginStatusAndNavigate();
        }
        private async Task CheckLoginStatusAndNavigate()
        {
            
            bool isLoggedIn = await _authService.IsUserLoggedInAsync();

            if (isLoggedIn)
            {
                await Shell.Current.GoToAsync($"//{nameof(Dashboard)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(GachaBanner)}");
            }
        }
    }
}