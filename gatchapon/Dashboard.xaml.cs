using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using gatchapon.Models;

  
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

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }
    private async void OnBannerTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("GachaBanner");
    }
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ProfileSetting");
    }
    private async void OnclickedQuest(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Quest");
    }

    private async void OnclickedCharacter(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Characters");
    }
    private async void OnclickedNews(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("News");
    }
}