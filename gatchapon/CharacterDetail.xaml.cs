using gatchapon.Models;
using Microsoft.Maui.ApplicationModel; // Added for SecureStorage

namespace gatchapon
{
    [QueryProperty(nameof(CharName), "name")]
    public partial class CharacterDetail : ContentPage
    {
        public string CharName { get; set; }
        private readonly FirebaseDatabaseService _dbService = new();

        public CharacterDetail()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadCharacterData();
        }

        private void LoadCharacterData()
        {
            if (string.IsNullOrEmpty(CharName)) return;

            NameLabel.Text = CharName;

            // 1. LOAD FULL BODY IMAGE
            // Assumes file names: "marisol_full.png", "roberto_full.png"
            FullBodyImage.Source = $"{CharName.ToLower()}_full.png";

            // 2. LOAD DESCRIPTION (You can customize this!)
            DescriptionLabel.Text = GetDescription(CharName);
        }

        private string GetDescription(string name)
        {
            // Simple switch to return descriptions. 
            // You could also move this to your 'knowledge_base' in Firebase later!
            return name switch
            {
                "Marisol" => "A top-tier flight attendant for Cloud-9 Airlines. Marisol treats every day like a flight plan—there might be turbulence, but she guarantees a safe landing. She loves paper planes because they remind her that even simple things can fly high.",
                "Roberto" => "The head chef of The Golden Spoon. Roberto believes that productivity is just like a good soup—it needs patience, the right ingredients, and a little bit of spice. He wears burger sneakers because he believes comfort is the key to success.",
                "Maxine" => "A freelance productivity hacker who spends 90% of her time online. Maxine treats real life like an RPG—tasks are just mobs to be farmed for XP. She might look messy, but her code (and her schedule) is clean."
            };
        }

        private async void OnChatClicked(object sender, EventArgs e)
        {
            // Navigate to Chat Page with this character
            await Shell.Current.GoToAsync($"{nameof(ChatPage)}?targetName={CharName}");
        }

        private async void OnEquipClicked(object sender, EventArgs e)
        {
            // FIX: Get the userId from SecureStorage before using it
            string userId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(userId))
            {
                await DisplayAlert("Error", "User not logged in.", "OK");
                return;
            }

            var user = await _dbService.GetUserAsync<UserModel>(userId);
            if (user != null)
            {
                user.EquippedCharacter = CharName; // <--- This sets the equipped character name
                await _dbService.SaveUserAsync(userId, user);
                await DisplayAlert("Equipped", $"{CharName} is now your companion!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Could not load user data to equip.", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}