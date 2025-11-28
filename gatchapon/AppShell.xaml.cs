using gatchapon.Models;

namespace gatchapon
{
    public partial class AppShell : Shell
    {
        private readonly FirebaseAuthService _authService = new FirebaseAuthService();

        public AppShell()
        {
            InitializeComponent();

            // --- REGISTER ROUTES (Only for pages NOT in the TabBar/Flyout) ---

            // Auth & Settings
            Routing.RegisterRoute(nameof(Register), typeof(Register));
            Routing.RegisterRoute(nameof(Login), typeof(Login));
            Routing.RegisterRoute(nameof(ForgotPass), typeof(ForgotPass));
            Routing.RegisterRoute(nameof(ProfileSetting), typeof(ProfileSetting));

            // Game Pages
            Routing.RegisterRoute(nameof(Characters), typeof(Characters));
            Routing.RegisterRoute(nameof(ResultPage), typeof(ResultPage));
            Routing.RegisterRoute(nameof(ResultPageSingle), typeof(ResultPageSingle));
            Routing.RegisterRoute(nameof(NamePage), typeof(NamePage));
            Routing.RegisterRoute(nameof(Inventory), typeof(Inventory));
            Routing.RegisterRoute(nameof(GachaBanner), typeof(GachaBanner));
            Routing.RegisterRoute(nameof(Shop), typeof(Shop));
            Routing.RegisterRoute(nameof(Quest), typeof(Quest));
            Routing.RegisterRoute(nameof(News), typeof(News));
            Routing.RegisterRoute(nameof(TodoPage), typeof(TodoPage));
            Routing.RegisterRoute(nameof(SocialPage), typeof(SocialPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
            Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
            Routing.RegisterRoute(nameof(FriendsPage), typeof(FriendsPage));
            Routing.RegisterRoute(nameof(CharacterDetail), typeof(CharacterDetail));
            Routing.RegisterRoute(nameof(ImageDisplayPage), typeof(ImageDisplayPage));
            
            // ❌ REMOVED: Routing.RegisterRoute(nameof(Dashboard)... 
            // WHY: Dashboard is already defined in AppShell.xaml with Route="DashboardPage"

            Dispatcher.Dispatch(async () => await CheckLoginStatusAndNavigate());
        }

        private async Task CheckLoginStatusAndNavigate()
        {
            try
            {
                bool isLoggedIn = await _authService.IsUserLoggedInAsync();

                if (isLoggedIn)
                {
                    // ✅ FIX: Use the Route name defined in AppShell.xaml ("DashboardPage")
                    // The "///" forces it to reset the stack and go to the main tab
                    await Shell.Current.GoToAsync("///DashboardPage");
                }
                else
                {
                    // ✅ LOGIC UPDATE: If not logged in, usually go to Login?
                    // If you really want GachaBanner, keep it, but Login is standard.
                    await Shell.Current.GoToAsync(nameof(Login));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation Error: {ex.Message}");
            }
        }
    }
}