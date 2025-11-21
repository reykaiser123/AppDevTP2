using gatchapon.Models;
using System.Collections.ObjectModel;

namespace gatchapon
{
    public partial class Characters : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private string _currentUserId;

        // CollectionView binds to this list
        public ObservableCollection<CharacterDisplayItem> CharacterList { get; set; } = new ObservableCollection<CharacterDisplayItem>();

        public Characters()
        {
            InitializeComponent();
            BindingContext = this; // Connect XAML to this C# file
        }
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _currentUserId = await SecureStorage.GetAsync("userId");
            if (!string.IsNullOrEmpty(_currentUserId))
            {
                await LoadCharacters();
            }
        }

        private async Task LoadCharacters()
        {
            try
            {
                // 1. Get User Data (to see what they unlocked)
                var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                var unlockedNames = user?.UnlockedCharacters ?? new List<string>();

                // 2. Clear current list
                CharacterList.Clear();

                // 3. Define ALL Game Characters here
                var allCharacters = new List<CharacterDefinition>
                {
                    // Name must match EXACTLY what you save in GachaBanner
                    new CharacterDefinition { Name = "Marisol", Rarity = "Epic" },
                    new CharacterDefinition { Name = "Maxine",  Rarity = "Rare" },
                    new CharacterDefinition { Name = "Roberto", Rarity = "Common" }
                };

                // 4. Loop and decide which image to show
                foreach (var charDef in allCharacters)
                {
                    bool isUnlocked = unlockedNames.Contains(charDef.Name);

                    // LOGIC: Switch Image based on lock state
                    // e.g., "marisol_char.png" vs "marisol_char_lock.png"
                    string imageFile = isUnlocked
                        ? $"{charDef.Name.ToLower()}_char.png"
                        : $"{charDef.Name.ToLower()}_char_lock.png";

                    // Optional: Gray border if locked, Rarity color if unlocked
                    Color borderColor = Colors.Gray;
                    if (isUnlocked)
                    {
                        if (charDef.Rarity == "Epic") borderColor = Color.FromArgb("#A335EE"); // Purple
                        else if (charDef.Rarity == "Rare") borderColor = Color.FromArgb("#0070DD"); // Blue
                        else borderColor = Color.FromArgb("#1EFF00"); // Green
                    }

                    // Add to the list for the UI
                    CharacterList.Add(new CharacterDisplayItem
                    {
                        Name = charDef.Name,
                        Image = imageFile,
                        BorderColor = borderColor
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Could not load characters.", "OK");
            }
        }
    }

    // Helper Class for Logic
    public class CharacterDefinition
    {
        public string Name { get; set; }
        public string Rarity { get; set; }
    }

    // Helper Class for UI Binding
    public class CharacterDisplayItem
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public Color BorderColor { get; set; }
    }
}