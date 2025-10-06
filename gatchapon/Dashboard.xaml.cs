using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
namespace gatchapon;

public partial class Dashboard : ContentPage
{
	public Dashboard()
	{
		InitializeComponent();
		

    }

    private async void OnclickedShop(object sender, EventArgs e)
    {

        await Shell.Current.GoToAsync("Shop");
    }

    private async void OnclickedProfile(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ProfileSetting");
    }
}