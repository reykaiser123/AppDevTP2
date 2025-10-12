using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using gatchapon;
using Microsoft.Maui.ApplicationModel;


namespace gatchapon;

public partial class ProfileSetting : ContentPage
{
	private readonly FirebaseAuthService _authService = new FirebaseAuthService();
	
    public ProfileSetting()
	{
		InitializeComponent();
	}

	public async void OnLogout(object? sender, EventArgs e)
	{
		bool confirm = await DisplayAlert("Log out", "Are you sure you want to log out?", "Yes", "No");
		if (confirm)
		{
			try
			{
				await _authService.LogOut();
				await Shell.Current.GoToAsync("//Login");
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Logout failed: {ex.Message}", "OK");
            }
        }
			
    }
}