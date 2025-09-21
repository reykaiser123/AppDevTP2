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

        }
    }
}
