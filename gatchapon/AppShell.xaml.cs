using gatchapon.Models;

namespace gatchapon
{
    public partial class AppShell : Shell
    {
        private readonly FirebaseAuthService _authService = new FirebaseAuthService();

        public AppShell()
        {
            InitializeComponent();

            // --- REGISTER ROUTES ---

            // Auth & Settings
            Routing.RegisterRoute(nameof(Register), typeof(Register));
            Routing.RegisterRoute(nameof(Login), typeof(Login));
            Routing.RegisterRoute(nameof(ForgotPass), typeof(ForgotPass));
            Routing.RegisterRoute(nameof(ProfileSetting), typeof(ProfileSetting));

            // Game Pages
            // ADDED: Characters route to fix the crash
            Routing.RegisterRoute(nameof(Characters), typeof(Characters));

            Routing.RegisterRoute(nameof(ResultPage), typeof(ResultPage));
            Routing.RegisterRoute(nameof(ResultPageSingle), typeof(ResultPageSingle));
            Routing.RegisterRoute(nameof(NamePage), typeof(NamePage));
            Routing.RegisterRoute(nameof(Inventory), typeof(Inventory));
            Routing.RegisterRoute(nameof(GachaBanner), typeof(GachaBanner));
            Routing.RegisterRoute(nameof(Dashboard), typeof(Dashboard));
            Routing.RegisterRoute(nameof(Shop), typeof(Shop));
            Routing.RegisterRoute(nameof(Quest), typeof(Quest));
            Routing.RegisterRoute(nameof(News), typeof(News));
            // NOTE: If 'Shop', 'Quest', or 'News' are NOT in your bottom tabs (AppShell.xaml),
            // you might need to register them here too if you get similar errors for them.
            // e.g.: Routing.RegisterRoute(nameof(Shop), typeof(Shop));

            Dispatcher.Dispatch(async () => await CheckLoginStatusAndNavigate());
        }

        private async Task CheckLoginStatusAndNavigate()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation Error: {ex.Message}");
            }
        }
    }
}