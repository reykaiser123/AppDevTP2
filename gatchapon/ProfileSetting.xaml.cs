using System.Threading.Tasks;

namespace gatchapon;

public partial class ProfileSetting : ContentPage
{
	public ProfileSetting()
	{
		InitializeComponent();
	}

	public async void OnLogout(object? sender, EventArgs e)
	{
        await Shell.Current.GoToAsync("//Login");


    }
}