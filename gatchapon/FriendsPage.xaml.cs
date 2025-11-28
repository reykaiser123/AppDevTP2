using Firebase.Database;
using Firebase.Database.Query;
using gatchapon.Models; // Assume FriendItem, FriendRequest, and UserModel are here
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

// NOTE: Ensure the ContactItem class is defined at the bottom of this file, 
// as it is specific to this page's CollectionView.

namespace gatchapon
{
    public partial class FriendsPage : ContentPage
    {
        private readonly FirebaseClient _firebaseClient = new FirebaseClient("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");
        private string _currentUserId;

        // This collection holds the custom ContactItem objects for the UI
        public ObservableCollection<ContactItem> Contacts { get; set; } = new ObservableCollection<ContactItem>();

        public FriendsPage()
        {
            InitializeComponent();
            // Assuming ContactsList is the x:Name of your CollectionView in XAML
            // ContactsList.ItemsSource = Contacts; 
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _currentUserId = await SecureStorage.GetAsync("userId");
            await LoadContacts();
        }

        private async Task LoadContacts()
        {
            Contacts.Clear();

            // 1. ADD MARISOL (The Bot)
            Contacts.Add(new ContactItem
            {
                Name = "Marisol",
                Image = "marisol_char.png",
                Status = "AI Companion",
                IsBot = true,
                TargetId = "BOT_MARISOL" // Add a unique ID for the bot, since TargetId is expected
            });

            // 2. ADD REAL FRIENDS
            try
            {
                // We fetch the data from the 'friends' node, which stores FriendItem objects
                var friendsData = await _firebaseClient
                    .Child("users")
                    .Child(_currentUserId)
                    .Child("friends")
                    .OnceAsync<FriendItem>(); // <-- Fetching the FriendItem model

                // List to keep track of IDs we already added (for added safety)
                var addedIds = new List<string> { "BOT_MARISOL" };

                foreach (var item in friendsData)
                {
                    var friend = item.Object;

                    // The FriendId is stored as the KEY in Firebase (due to PutAsync fix)
                    friend.FriendId = item.Key;

                    if (addedIds.Contains(friend.FriendId)) continue;
                    addedIds.Add(friend.FriendId);

                    // FETCH REAL PROFILE PICTURE (Optional but good practice)
                    string displayImage = friend.Image ?? "profile.png"; // Use stored image or default

                    try
                    {
                        var friendProfile = await _firebaseClient
                            .Child("users")
                            .Child(friend.FriendId)
                            .OnceSingleAsync<UserModel>();

                        if (friendProfile != null && !string.IsNullOrEmpty(friendProfile.ProfilePictureUrl))
                        {
                            displayImage = friendProfile.ProfilePictureUrl;
                        }
                    }
                    catch { } // Ignore profile picture errors

                    Contacts.Add(new ContactItem
                    {
                        Name = friend.Name,
                        TargetId = friend.FriendId,
                        Image = displayImage,
                        Status = "Friend",
                        IsBot = false
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading real friends: {ex.Message}");
            }
        }

        private async void OnContactTapped(object sender, TappedEventArgs e)
        {
            var contact = e.Parameter as ContactItem;
            if (contact == null) return;

            // Determine if it's a bot chat or human chat
            string targetId = contact.IsBot ? null : contact.TargetId;
            string targetName = contact.Name;

            // If it's a bot, we only pass the name, relying on ChatPage to load the equipped char logic.
            // If it's a human, we pass the TargetId to trigger human chat mode.

            await Shell.Current.GoToAsync($"{nameof(ChatPage)}?targetId={targetId}&targetName={targetName}");
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // --- START OF REQUIRED HELPER CLASSES ---

        // Helper Class: Specific to this page's CollectionView UI binding
       

        // IMPORTANT: The definition for FriendItem and FriendRequest MUST be removed from here 
        // and placed ONLY in your Models folder.
    }
}