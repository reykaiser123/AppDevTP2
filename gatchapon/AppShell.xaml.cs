namespace gatchapon
{
    public partial class AppShell : Shell
    {
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

        }
    }
}
 