using System.Threading.Tasks;

namespace LoginPage
{
    public partial class Login : ContentPage
    {

        public Login()
        {
            InitializeComponent();
        }

        private async Task OnSignIn(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProfileSetting");
        }
    }
}
