using Firebase.Database;
using Firebase.Database.Query;
using gatchapon.Models;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;

namespace gatchapon
{
    public partial class ProfileSetting : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new();
        private string _currentUserId;
        private UserModel _currentUser;

        // --- INITIALIZATION ---
        private readonly FirebaseClient _firebaseClient = new FirebaseClient("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");
        public ObservableCollection<FriendItem> FriendsList { get; set; } = new ObservableCollection<FriendItem>();
        // --- END INITIALIZATION ---

        public ProfileSetting()
        {
            InitializeComponent();
            // Ensure you have bound this CollectionView in XAML: x:Name="FriendsCollectionView"
            FriendsCollectionView.ItemsSource = FriendsList;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _currentUserId = await SecureStorage.GetAsync("userId");
            await LoadUserProfile();
            await LoadFriendsList(); // Load friends list first
            LoadSettingsState();
        }

        // --- NEW: LOAD FRIENDS LIST METHOD (FIXES DUPLICATION ISSUE) ---
        private async Task LoadFriendsList()
        {
            FriendsList.Clear();
            if (string.IsNullOrEmpty(_currentUserId)) return;

            try
            {
                // Retrieve friends using OnceAsync
                var friendsData = await _firebaseClient
                    .Child("users")
                    .Child(_currentUserId)
                    .Child("friends")
                    .OnceAsync<FriendItem>();

                foreach (var item in friendsData)
                {
                    // The item.Key is the actual FriendId (from the PutAsync fix in NotificationsPage)
                    FriendsList.Add(new FriendItem
                    {
                        FriendId = item.Key,
                        Name = item.Object.Name
                    });
                }

                // Update the visible friend count label based on the actual list size
                if (FriendsLabel != null)
                {
                    FriendsLabel.Text = $"{FriendsList.Count} Friends";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading friends list: {ex.Message}");
                if (FriendsLabel != null) FriendsLabel.Text = "Error Loading Friends";
            }
        }
        // --- END LOAD FRIENDS LIST METHOD ---

        // --- NEW: CHAT FRIEND HANDLER ---
        private async void OnChatFriendClicked(object sender, EventArgs e)
        {
            var friend = (sender as Button)?.CommandParameter as FriendItem;
            if (friend == null) return;

            // Navigate to the ChatPage, passing the user ID to trigger _isHumanChat = true
            await Shell.Current.GoToAsync($"{nameof(ChatPage)}?targetId={friend.FriendId}&targetName={friend.Name}");
        }
        // --- END CHAT FRIEND HANDLER ---


        private void LoadSettingsState()
        {
            bool isDark = Preferences.Get("isDarkMode", false);
            DarkModeSwitch.IsToggled = isDark;
            bool isNotifEnabled = Preferences.Get("isNotifEnabled", true);
            NotifSwitch.IsToggled = isNotifEnabled;
        }

        private void OnDarkModeToggled(object sender, ToggledEventArgs e)
        {
            bool isDark = e.Value;
            Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
            Preferences.Set("isDarkMode", isDark);
        }

        private async void OnNotificationsToggled(object sender, ToggledEventArgs e)
        {
            bool isEnabled = e.Value;
            Preferences.Set("isNotifEnabled", isEnabled);
            if (isEnabled) await DisplayAlert("Notifications", "Notifications ON", "OK");
            else await DisplayAlert("Notifications", "Notifications OFF", "OK");
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async Task LoadUserProfile()
        {
            if (string.IsNullOrEmpty(_currentUserId)) return;

            try
            {
                _currentUser = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                if (_currentUser != null)
                {
                    Userlabel.Text = !string.IsNullOrEmpty(_currentUser.Username) ? _currentUser.Username : "Traveler";
                    EmailDisplayLabel.Text = !string.IsNullOrEmpty(_currentUser.Email) ? _currentUser.Email : "No Email";

                    if (!string.IsNullOrEmpty(_currentUser.PhoneNumber)) PhoneLabel.Text = _currentUser.PhoneNumber;
                    else PhoneLabel.Text = "No Phone Set";

                    // Note: FriendsLabel text is updated by LoadFriendsList() to use the actual list count.
                    // The line below uses the stored count, which may be outdated, so LoadFriendsList is preferred.
                    // if (FriendsLabel != null) FriendsLabel.Text = $"{_currentUser.FriendsCount} Friends"; 

                    if (!string.IsNullOrEmpty(_currentUser.ProfilePictureUrl))
                    {
                        if (_currentUser.ProfilePictureUrl.Contains("/") || _currentUser.ProfilePictureUrl.Contains("\\"))
                            AvatarButton.Source = ImageSource.FromFile(_currentUser.ProfilePictureUrl);
                        else
                            AvatarButton.Source = _currentUser.ProfilePictureUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load profile", "OK");
            }
        }

        private async void Cnamebtn(object sender, EventArgs e)
        {
            string newName = await DisplayPromptAsync("Change Username", "Enter your new username:");
            if (!string.IsNullOrWhiteSpace(newName))
            {
                await _dbService.UpdateUserFieldAsync(_currentUserId, "username", newName);
                Userlabel.Text = newName;
                await DisplayAlert("Success", "Username updated!", "OK");
            }
        }

        private async void Cphonebtn(object sender, EventArgs e)
        {
            string newPhone = await DisplayPromptAsync("Change Phone", "Enter new phone number:", keyboard: Keyboard.Telephone);
            if (!string.IsNullOrWhiteSpace(newPhone))
            {
                await _dbService.UpdateUserFieldAsync(_currentUserId, "phoneNumber", newPhone);
                PhoneLabel.Text = newPhone;
                await DisplayAlert("Success", "Phone number updated!", "OK");
            }
        }

        private async void OnUpdateEmailClicked(object sender, EventArgs e)
        {
            string newEmail = await DisplayPromptAsync("Update Email", "Enter new email address:", keyboard: Keyboard.Email);
            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail.Contains("@"))
            {
                await _dbService.UpdateUserFieldAsync(_currentUserId, "email", newEmail);
                EmailDisplayLabel.Text = newEmail;
                await DisplayAlert("Success", "Email updated!", "OK");
            }
            else if (newEmail != null)
            {
                await DisplayAlert("Invalid", "Please enter a valid email.", "OK");
            }
        }

        private async void OnUpdatePasswordClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Reset Password", "Send a password reset email?", "Yes", "Cancel");
            if (confirm) await DisplayAlert("Sent", "Check your email inbox.", "OK");
        }

        private async void OnProf(object sender, EventArgs e)
        {
            // 1. Get the URL of the CURRENTLY DISPLAYED IMAGE
            string currentImageUrl = _currentUser?.ProfilePictureUrl;

            // 2. Build the Menu Options
            List<string> actions = new List<string> { "Pick from Device", "Select Character Avatar" };

            // Add a "View Current" option ONLY if a picture is set
            if (!string.IsNullOrEmpty(currentImageUrl))
            {
                actions.Insert(0, "View Current Picture");
            }

            string action = await DisplayActionSheet("Change Profile Picture", "Cancel", null, actions.ToArray());

            if (action == "View Current Picture")
            {
                await Shell.Current.GoToAsync("ImageDisplayPage?imageUrl=" + Uri.EscapeDataString(currentImageUrl));
                return;
            }

            // --- EXISTING LOGIC STARTS HERE ---
            if (action == "Pick from Device")
            {
                try
                {
                    var result = await MediaPicker.Default.PickPhotoAsync();
                    if (result != null)
                    {
                        string localPath = result.FullPath;
                        await _dbService.UpdateUserFieldAsync(_currentUserId, "profilePictureUrl", localPath);
                        AvatarButton.Source = ImageSource.FromFile(localPath);
                        await DisplayAlert("Success", "Profile picture updated!", "OK");
                    }
                }
                catch (Exception ex) { /* Permission denied or cancelled */ }
            }
            else if (action == "Select Character Avatar")
            {
                if (_currentUser == null || _currentUser.UnlockedCharacters == null || _currentUser.UnlockedCharacters.Count == 0)
                {
                    await DisplayAlert("No Characters", "You haven't unlocked any characters yet!", "OK");
                    return;
                }

                string charAction = await DisplayActionSheet("Select Avatar", "Cancel", null, _currentUser.UnlockedCharacters.ToArray());

                if (charAction != "Cancel" && charAction != null)
                {
                    string imageFile = $"{charAction.ToLower()}_char.png";
                    await _dbService.UpdateUserFieldAsync(_currentUserId, "profilePictureUrl", imageFile);
                    AvatarButton.Source = imageFile;
                    await DisplayAlert("Updated", $"Profile picture set to {charAction}!", "OK");
                }
            }
        }

        public async void OnLogout(object? sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Log out", "Are you sure you want to log out?", "Yes", "No");
            if (confirm)
            {
                SecureStorage.Remove("userId");
                SecureStorage.Remove("userName");
                await _authService.LogOut();
                await DisplayAlert("Logged Out", "See you next time!", "OK");
                await Shell.Current.GoToAsync("//Login");
            }
        }

        private async void OnAppV(object sender, TappedEventArgs e)
        {
            await DisplayAlert("App Version", "Gatchapon Quest v0.1 \nBuild: Alpha", "OK");
        }

        private async void OnSup(object sender, TappedEventArgs e)
        {
            await DisplayAlert("Support", "Contact us at support@gatchapon.com", "OK");
        }

    }
}