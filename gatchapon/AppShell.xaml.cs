namespace gatchapon
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("ProfileSetting", typeof(ProfileSetting));
            Routing.RegisterRoute("Login", typeof(Login));
        }
    }
}
 