using System.Threading.Tasks;

namespace gatchapon;

public partial class ProfileSetting : ContentPage
{
	public ProfileSetting()
	{
		InitializeComponent();
	}

	public void OnlogoutClicked(object? sender, EventArgs e)
	{
		Shell.Current.GoToAsync("Login");
    }
}