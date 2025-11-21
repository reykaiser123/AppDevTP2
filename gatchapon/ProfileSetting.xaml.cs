using gatchapon.Models;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Media;
using System.Linq;

namespace gatchapon
{
    public partial class ProfileSetting : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new();
        private string _currentUserId;
        private UserModel _currentUser;

        public ProfileSetting()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _currentUserId = await SecureStorage.GetAsync("userId");
            await LoadUserProfile();
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
                    // Update UI with database values
                    Userlabel.Text = !string.IsNullOrEmpty(_currentUser.Username) ? _currentUser.Username : "Traveler";
                    EmailDisplayLabel.Text = !string.IsNullOrEmpty(_currentUser.Email) ? _currentUser.Email : "No Email";

                    // Load Phone Number
                    if (!string.IsNullOrEmpty(_currentUser.PhoneNumber))
                        PhoneLabel.Text = _currentUser.PhoneNumber;
                    else
                        PhoneLabel.Text = "No Phone Set";

                    // Load Profile Picture
                    if (!string.IsNullOrEmpty(_currentUser.ProfilePictureUrl))
                    {
                        // Check if it's a local file path (from device) or a resource name (from game)
                        if (_currentUser.ProfilePictureUrl.Contains("/") || _currentUser.ProfilePictureUrl.Contains("\\"))
                        {
                            // It's a file path from the device
                            ProfileImageButton.Source = ImageSource.FromFile(_currentUser.ProfilePictureUrl);
                        }
                        else
                        {
                            // It's a character name from resources (e.g., "marisol_char.png")
                            ProfileImageButton.Source = _currentUser.ProfilePictureUrl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load profile", "OK");
            }
        }

        // --- 1. CHANGE USERNAME ---
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

        // --- 2. CHANGE PHONE NUMBER ---
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

        // --- 3. CHANGE EMAIL ---
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

        // --- 4. CHANGE PASSWORD ---
        private async void OnUpdatePasswordClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Reset Password", "Send a password reset email to your address?", "Yes", "Cancel");
            if (confirm)
            {
                await DisplayAlert("Sent", "Check your email inbox.", "OK");
            }
        }

        // --- 5. CHANGE PROFILE PICTURE ---
        private async void OnProf(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Change Profile Picture", "Cancel", null, "Pick from Device", "Select Character Avatar");

            if (action == "Pick from Device")
            {
                try
                {
                    var result = await MediaPicker.Default.PickPhotoAsync();
                    if (result != null)
                    {
                        string localPath = result.FullPath;
                        await _dbService.UpdateUserFieldAsync(_currentUserId, "profilePictureUrl", localPath);
                        ProfileImageButton.Source = ImageSource.FromFile(localPath);
                        await DisplayAlert("Success", "Profile picture updated from device!", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Permission denied or cancelled
                }
            }
            else if (action == "Select Character Avatar")
            {
                if (_currentUser == null || _currentUser.UnlockedCharacters == null || _currentUser.UnlockedCharacters.Count == 0)
                {
                    await DisplayAlert("No Characters", "You haven't unlocked any characters yet! Pull from the Gacha to get avatars.", "OK");
                    return;
                }

                string charAction = await DisplayActionSheet("Select Avatar", "Cancel", null, _currentUser.UnlockedCharacters.ToArray());

                if (charAction != "Cancel" && charAction != null)
                {
                    string imageFile = $"{charAction.ToLower()}_char.png";
                    await _dbService.UpdateUserFieldAsync(_currentUserId, "profilePictureUrl", imageFile);
                    ProfileImageButton.Source = imageFile;
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
                Application.Current.MainPage = new AppShell();
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