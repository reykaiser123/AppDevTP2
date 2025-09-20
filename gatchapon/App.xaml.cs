namespace gatchapon
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
          //  MainPage = new Login();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}