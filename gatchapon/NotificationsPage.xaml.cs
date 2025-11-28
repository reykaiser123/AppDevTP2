using Firebase.Database;
using Firebase.Database.Query;
using gatchapon.Models; // Assumes UserModel is here
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.Linq; // Added for convenience, though not strictly required here

// NOTE: This file assumes the UserModel class is defined in gatchapon.Models

namespace gatchapon
{
    public partial class NotificationsPage : ContentPage
    {
        private readonly FirebaseClient _firebaseClient = new FirebaseClient("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");
        private string _currentUserId;

        // This MUST be the centralized model now
        public ObservableCollection<FriendRequest> Requests { get; set; } = new ObservableCollection<FriendRequest>();

        public NotificationsPage()
        {
            InitializeComponent();
            // ... (rest of constructor is fine)
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _currentUserId = await SecureStorage.GetAsync("userId");
            await LoadRequests();
        }

        private async Task LoadRequests()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentUserId)) return;

                var items = await _firebaseClient
                    .Child("users")
                    .Child(_currentUserId)
                    .Child("friend_requests_received")
                    .OnceAsync<FriendRequest>();

                Requests.Clear();
                foreach (var item in items)
                {
                    var req = item.Object;
                    // FIX: The Key property is retrieved from the Firebase item response
                    req.Key = item.Key; // This line now works because Key is defined below
                    Requests.Add(req);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading requests: {ex.Message}");
            }
        }

        private async void OnAcceptClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var request = button?.CommandParameter as FriendRequest;
            if (request == null) return;

            try
            {
                string myName = await SecureStorage.GetAsync("userName") ?? "Unknown";

                // 1. Add to MY friends list (I see Them) - USING PUTASYNC WITH FRIEND ID AS KEY (Prevents duplicates)
                await _firebaseClient
                    .Child("users")
                    .Child(_currentUserId)
                    .Child("friends")
                    .Child(request.FromId)
                    .PutAsync(new { Name = request.FromName }); // Firebase will save FriendId as the key

                // 2. Add ME to THEIR friends list (They see Me) - USING PUTASYNC WITH MY ID AS KEY (Prevents duplicates)
                await _firebaseClient
                    .Child("users")
                    .Child(request.FromId)
                    .Child("friends")
                    .Child(_currentUserId)
                    .PutAsync(new { Name = myName });

                // 3. Update Counts 
                await UpdateCount(_currentUserId);
                await UpdateCount(request.FromId);

                // 4. Delete Request
                await DeleteRequest(request);

                await DisplayAlert("Success", $"You are now friends with {request.FromName}!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to accept friend: {ex.Message}", "OK");
            }
        }

        private async void OnDeclineClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var request = button?.CommandParameter as FriendRequest;
            if (request != null)
            {
                await DeleteRequest(request);
            }
        }

        private async Task DeleteRequest(FriendRequest request)
        {
            try
            {
                // Deletes the request using the Firebase key saved in the model
                await _firebaseClient
                    .Child("users")
                    .Child(_currentUserId)
                    .Child("friend_requests_received")
                    .Child(request.Key) // Uses the Key property
                    .DeleteAsync();

                Requests.Remove(request);
            }
            catch { }
        }

        private async Task UpdateCount(string userId)
        {
            // Note: This needs revision to handle incrementing/decrementing more robustly
            // but is kept minimal to resolve current issues.
            try
            {
                var user = await _firebaseClient.Child("users").Child(userId).OnceSingleAsync<UserModel>();
                if (user != null)
                {
                    // This logic should ideally calculate the *actual* friend list count
                    // for robustness, but we increment it here based on your existing code:
                    int newCount = user.FriendsCount + 1;

                    // Update just the specific field safely
                    await _firebaseClient.Child("users").Child(userId).Child("FriendsCount").PutAsync(newCount);
                }
            }
            catch { }
        }

        // Recommended Shell navigation (Safely pops the current page)
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await NavigationHelper.SafeGoToAsync("..");
        }
    }

    // ===================================================================
    // CENTRALIZED MODEL DEFINITIONS (Must be defined here or in Models folder)
    // ===================================================================

    // NOTE: If you define these models here, DELETE them from all other files 
    // (SocialPage.xaml.cs, etc.) to prevent compilation errors.

    // **********************************
    // THE MISSING CLASS ADDED HERE TO FIX THE ERROR
    // **********************************
    public class FriendRequest
    {
        // This is the unique key generated by Firebase (e.g., -M-ABC123...)
        public string Key { get; set; }

        // The ID of the user who SENT the request
        public string FromId { get; set; }

        // The Name of the user who SENT the request
        public string FromName { get; set; }

        // You may want to include a timestamp or other details
        // public DateTime DateSent { get; set; } 
    }

    // FriendItem model used in other pages (like ProfileSetting/FriendsPage)
    // If you already have this in a Models folder, DELETE this definition.
    public class FriendItem
    {
        public string Name { get; set; }
        public string FriendId { get; set; }
        public string Image { get; set; }
        public string Status { get; set; }
        public bool IsBot { get; set; }
    }
}