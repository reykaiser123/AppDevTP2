using Firebase.Database;
using Firebase.Database.Query;
using gatchapon.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace gatchapon
{
    public partial class SocialPage : ContentPage
    {
        private readonly FirebaseClient _firebaseClient;
        private string _currentUserId;
        private string _currentUserName;

        private List<UserModel> _allUsersSource = new List<UserModel>();
        public ObservableCollection<UserItem> DisplayedUsers { get; set; } = new ObservableCollection<UserItem>();

        private HashSet<string> _myFriendIds = new HashSet<string>();
        private HashSet<string> _myPendingRequestIds = new HashSet<string>();

        public SocialPage()
        {
            InitializeComponent();
            _firebaseClient = new FirebaseClient("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");

            BindingContext = this;

            // FIX — Bind list to CollectionView
            UserCollectionView.ItemsSource = DisplayedUsers;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _currentUserId = await SecureStorage.GetAsync("userId");
            _currentUserName = await SecureStorage.GetAsync("userName");

            await LoadMyFriendIds();
            await LoadMyPendingRequestIds();
            await PreloadUsers();

            // first search run
            RefreshSearch(UserSearchEntry.Text);
        }
        private async Task LoadMyPendingRequestIds()
        {
            _myPendingRequestIds.Clear();
            if (string.IsNullOrEmpty(_currentUserId)) return;

            // Note: Assumes you save requests sent on your own profile under 'requests_sent'
            var requestsSent = await _firebaseClient
                .Child("users")
                .Child(_currentUserId)
                .Child("requests_sent") // <--- NEW NODE YOU NEED TO ADD WHEN SENDING REQUEST
                .OnceAsync<object>();

            foreach (var r in requestsSent)
                _myPendingRequestIds.Add(r.Key);
        }

        private async Task LoadMyFriendIds()
        {
            _myFriendIds.Clear();
            if (string.IsNullOrEmpty(_currentUserId)) return;

            var friends = await _firebaseClient
                .Child("users")
                .Child(_currentUserId)
                .Child("friends")
                .OnceAsync<object>();

            foreach (var f in friends)
                _myFriendIds.Add(f.Key);
        }

        private async Task PreloadUsers()
        {
            var users = await _firebaseClient.Child("users").OnceAsync<UserModel>();
            _allUsersSource.Clear();

            foreach (var u in users)
            {
                var data = u.Object;
                data.UserId = u.Key;

                if (data.UserId == _currentUserId) continue;
                if (string.IsNullOrEmpty(data.Username)) continue;

                _allUsersSource.Add(data);
            }
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshSearch(e.NewTextValue);
        }

        // FIX — Extracted searchable logic
        private void RefreshSearch(string query)
        {
            query = query?.ToLower() ?? "";

            DisplayedUsers.Clear();

            if (string.IsNullOrWhiteSpace(query))
                return;

            var results = _allUsersSource
                .Where(u => u.Username.ToLower().Contains(query));

            foreach (var u in results)
            {
                bool isFriend = _myFriendIds.Contains(u.UserId);
                bool isPending = _myPendingRequestIds.Contains(u.UserId);

                string status = "Add Friend";
                if (isFriend)
                    status = "Friends";
                else if (isPending)
                    status = "Requested";

                DisplayedUsers.Add(new UserItem
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    ProfilePictureUrl = u.ProfilePictureUrl ?? "profile_placeholder.png",
                    FriendStatus = status
                });
            }
        }

        private async void OnAddFriendClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var item = btn?.CommandParameter as UserItem;
            if (item == null) return;

            if (item.FriendStatus == "Friends" || item.FriendStatus == "Requested")
                return;

            btn.Text = "Requested";
            btn.IsEnabled = false;
            btn.BackgroundColor = Color.Parse("#FFC107");

            try
            {
                string myName = await SecureStorage.GetAsync("userName");

                await _firebaseClient
                    .Child("users")
                    .Child(item.UserId)
                    .Child("friend_requests_received")
                    .PostAsync(new { FromId = _currentUserId, FromName = myName });

                await _firebaseClient // <--- NEW WRITE
                    .Child("users")
                    .Child(_currentUserId)
                    .Child("requests_sent")
                    .Child(item.UserId)
                    .PutAsync(new { Sent = DateTime.UtcNow });
              
                item.FriendStatus = "Requested";

                await DisplayAlert("Success", "Friend request sent!", "OK");
            }
            catch
            {
                btn.Text = "Add Friend";
                btn.IsEnabled = true;
                btn.BackgroundColor = Color.Parse("#8B7E74");
            }
        }

        private async void OnChatClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var user = button?.CommandParameter as UserItem;

            if (user == null || user.FriendStatus != "Friends")
                return;

            await Shell.Current.GoToAsync($"{nameof(ChatPage)}?targetId={user.UserId}&targetName={user.Username}");
        }

        private void OnChatUserClicked(object sender, EventArgs e)
        {
            // TODO: Implement chat logic here
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await NavigationHelper.SafeGoToAsync("..");
        }

        public class UserItem
        {
            public string UserId { get; set; }
            public string Username { get; set; }
            public string ProfilePictureUrl { get; set; }
            public string FriendStatus { get; set; }
        }

    }

}