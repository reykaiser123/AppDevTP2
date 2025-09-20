namespace gatchapon
{
    public partial class Login : ContentPage
    {
        int count = 0;

        public Login()
        {
            InitializeComponent();
        }

        private void OnCountClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                counter.Text = $"Clicked {count} time";
            else
                counter.Text = $"Clicked {count} times";
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            string username = usernameEntry.Text;
            string password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }

            // Simple fake check (replace with real logic later)
            if (username == "admin" && password == "1234")
                DisplayAlert("Success", "Login successful!", "OK");
            else
                DisplayAlert("Failed", "Invalid username or password", "OK");
        }
    }
}
