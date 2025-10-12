using Microsoft.Maui.ApplicationModel.Communication;

namespace gatchapon;

public partial class ForgotPass : ContentPage
{
    private readonly FirebaseAuthService _authService = new FirebaseAuthService();

    public ForgotPass()
    {
        InitializeComponent();
    }

    private async void OnSendResetEmailClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            StatusLabel.Text = "Please enter your email.";
            return;
        }

        try
        {
            bool success = await _authService.SendPasswordResetEmailAsync(email);
            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = "Reset link sent. Check your email.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex.Message; // show Firebase’s real message
        }
    }
}