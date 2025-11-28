using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Firebase.Database;
using Firebase.Database.Query;
using gatchapon.Models;
using System;
using Microsoft.Maui.Controls;

namespace gatchapon
{
    public partial class Dashboard : ContentPage
    {
        private bool _isNavigating = false;

        private readonly FirebaseAuthService _authService = new();
        private FirebaseClient _firebaseClient;
        public ObservableCollection<UserTask> TodayTasks { get; set; } = new ObservableCollection<UserTask>();

        private string _currentUserId;
        private IDisposable _taskSubscription;
        private readonly FirebaseDatabaseService _dbService = new();

        public Dashboard()
        {
            InitializeComponent();
            this.BindingContext = this;
            _firebaseClient = new FirebaseClient("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 1. Load Data
            await LoadTasks();
            await LoadActiveCompanion(); // This will now load the correct image/name to the button

            // 2. Engagement Logic (Notifications)
            var notifService = new NotificationService();
            notifService.ScheduleDailyReminder();
            TodayTasks = new ObservableCollection<UserTask>();

            try
            {
                if (!string.IsNullOrEmpty(_currentUserId))
                {
                    var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);

                    if (user != null && user.TasksCompletedToday < 3)
                    {
                        notifService.ScheduleTaskReminder();
                    }
                    else
                    {
                        notifService.CancelTaskReminder();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking tasks: {ex.Message}");
            }

            // 2. Dynamically create/load tasks
            var firebaseTasks = await _firebaseClient.Child("tasks").Child(_currentUserId).OnceAsync<UserTask>();

            foreach (var firebaseTask in firebaseTasks)
            {
                var task = new UserTask
                {
                    TaskId = Guid.NewGuid().ToString(),
                    TaskName = "Daily Gold Task",
                    Reward = 100,
                    IsCompletedToday = false                     // ✅ reset
                };

                
            }

            // 3. Bind to CollectionView
            TasksCollectionView.ItemsSource = TodayTasks;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _taskSubscription?.Dispose();
            TodayTasks.Clear();
        }

        // --- COMPANION FACE LOGIC ---
        private async Task LoadActiveCompanion()
        {
            _currentUserId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(_currentUserId)) return;

            try
            {
                var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);

                string characterImage = "chat_bubble_default.png";
                string charName = "LOCKED";

                if (user != null && !string.IsNullOrEmpty(user.EquippedCharacter))
                {
                    // Use the equipped character's name to find the asset
                    characterImage = $"{user.EquippedCharacter.ToLower()}_char.png";
                    charName = user.EquippedCharacter;
                }

                if (CompanionButton != null)
                {
                    CompanionButton.Source = characterImage;
                    // Note: We don't actually need the CommandParameter anymore, 
                    // but keeping it here for debugging/future reference.
                    CompanionButton.CommandParameter = charName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading companion: {ex.Message}");
            }
        }

        // --- UPDATED CHAT HANDLER ---
        // This is already correct, using the new logic where ChatPage defaults to equipped char
        private async void OnCompanionChatClicked(object sender, EventArgs e)
        {
            await Task.Delay(100);
            await Shell.Current.GoToAsync(nameof(ChatPage));
        }
        // --- END UPDATED CHAT HANDLER ---


        // --- TASK LOGIC ---
        private async Task LoadTasks()
        {
            _currentUserId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(_currentUserId)) return;

            TodayTasks.Clear();

            _taskSubscription = _firebaseClient
                .Child("tasks")
                .Child(_currentUserId)
                .AsObservable<UserTask>()
                .Subscribe(d =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (d.Object != null)
                        {
                            var task = d.Object;
                            task.TaskId = d.Key;

                            var existingTask = TodayTasks.FirstOrDefault(t => t.TaskId == task.TaskId);

                            if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                            {
                                if (existingTask == null) TodayTasks.Add(task);
                                else TodayTasks[TodayTasks.IndexOf(existingTask)] = task;
                            }
                            else if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                            {
                                if (existingTask != null) TodayTasks.Remove(existingTask);
                            }
                        }
                    });
                });
        }

        private async void OnClaimRewardClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var task = button?.CommandParameter as UserTask;

            if (task == null || task.IsCompletedToday)
            {
                await DisplayAlert("Already Claimed", "You have already claimed this task for today.", "OK");
                return;
            }

            task.IsCompletedToday = true;
            await _firebaseClient.Child("tasks").Child(_currentUserId).Child(task.TaskId).PutAsync(task);

            try
            {
                var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                if (user != null)
                {
                    user.Gold += task.Reward;

                    string todayString = DateTime.Today.ToString("o");
                    if (user.LastTaskCompletionDate != todayString)
                    {
                        user.TasksCompletedToday = 1;
                        user.LastTaskCompletionDate = todayString;
                    }
                    else
                    {
                        user.TasksCompletedToday++;
                    }

                    await _dbService.SaveUserAsync(_currentUserId, user);
                    await DisplayAlert("Reward Claimed!", $"You earned {task.Reward} gold!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to give reward: {ex.Message}", "OK");
            }

            button.IsEnabled = false;
            button.Text = "Claimed";
            button.BackgroundColor = Color.FromArgb("#4CAF50");

        }

        private async void OnTasksClicked(object sender, EventArgs e)
        {
            await Task.Delay(100);
            await NavigationHelper.SafeGoToAsync(nameof(TodoPage));
        }

        private async void OnTaskTapped(object sender, TappedEventArgs e)
        {
            var task = e.Parameter as UserTask;
            if (task == null) return;

            string action = await DisplayActionSheet("Task Options", "Cancel", "Delete", "Edit");

            if (action == "Edit") await NavigationHelper.SafeGoToAsync(nameof(TodoPage));
            else if (action == "Delete")
            {
                bool confirm = await DisplayAlert("Delete Task", $"Delete '{task.TaskName}'?", "Yes", "No");
                if (confirm) await DeleteTask(task);
            }
        }

        private async Task DeleteTask(UserTask task)
        {
            if (string.IsNullOrEmpty(_currentUserId) || string.IsNullOrEmpty(task.TaskId)) return;
            try
            {
                await _firebaseClient.Child("tasks").Child(_currentUserId).Child(task.TaskId).DeleteAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete task: {ex.Message}", "OK");
            }
        }

        // --- NAVIGATION ---
        private async void OnNotificationsClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("NotificationsPage");
            }
            finally
            {
                _isNavigating = false;
            }
            
        }
        private async void OnCommunityClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("SocialPage");
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private async void OnclickedShop(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("Shop");
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) { }
        private async void OnBannerTapped(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("GachaBanner");
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private async void OnProfileClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("ProfileSetting");
            }
            finally
            {
                _isNavigating = false;
            }
            
        }
        private async void OnclickedQuest(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("Quest");
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private async void OnclickedCharacter(object? sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("Characters");
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private async void OnclickedNews(object? sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("News");
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private async void OnClickedInventory(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Shell.Current.GoToAsync("Inventory");
            }
            finally
            {
                _isNavigating = false;
            }
        }
    }
}