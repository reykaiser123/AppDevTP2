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
            await LoadTasks();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _taskSubscription?.Dispose();
            TodayTasks.Clear();
        }

        private async Task LoadTasks()
        {
            _currentUserId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(_currentUserId))
            {
                return;
            }

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
                                if (existingTask == null)
                                {
                                    TodayTasks.Add(task);
                                }
                                else
                                {
                                    int index = TodayTasks.IndexOf(existingTask);
                                    TodayTasks[index] = task;
                                }
                            }
                            else if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                            {
                                if (existingTask != null)
                                {
                                    TodayTasks.Remove(existingTask);
                                }
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

            // 1. Update the Task
            task.IsCompletedToday = true;
            // We need to save the date it was completed
            task.LastUpdated = DateTime.Today.ToString("o"); // <-- REQUIRES 'public string LastUpdated { get; set; }' in UserTask.cs

            await _firebaseClient
                .Child("tasks")
                .Child(_currentUserId)
                .Child(task.TaskId)
                .PutAsync(task);

            // 2. Give Gold Reward & Update Quest Counter
            try
            {
                var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                if (user != null)
                {
                    // --- Give Gold ---
                    user.Gold += task.Reward;

                    // --- Update Quest Counter ---
                    string todayString = DateTime.Today.ToString("o");
                    if (user.LastTaskCompletionDate != todayString)
                    {
                        // First task of the day, reset count
                        user.TasksCompletedToday = 1;
                        user.LastTaskCompletionDate = todayString;
                    }
                    else
                    {
                        // Not the first task, increment
                        user.TasksCompletedToday++;
                    }

                    // --- Save User ---
                    await _dbService.SaveUserAsync(_currentUserId, user);
                    await DisplayAlert("Reward Claimed!", $"You earned {task.Reward} gold!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to give reward: {ex.Message}", "OK");
            }

            // 3. Update Button State
            button.IsEnabled = false;
            button.Text = "Claimed";
            button.BackgroundColor = Color.FromArgb("#4CAF50"); // Green color
        }
        // --- ALL OTHER METHODS ARE UNCHANGED ---

        private async void OnTasksClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TodoPage());
        }

        private async void OnTaskTapped(object sender, TappedEventArgs e)
        {
            var task = e.Parameter as UserTask;
            if (task == null) return;

            string action = await DisplayActionSheet(
                "Task Options",
                "Cancel",
                "Delete",
                "Edit");

            switch (action)
            {
                case "Edit":
                    await Navigation.PushAsync(new TodoPage(task));
                    break;

                case "Delete":
                    bool confirm = await DisplayAlert(
                        "Delete Task",
                        $"Are you sure you want to delete '{task.TaskName}'?",
                        "Yes, Delete",
                        "No");

                    if (confirm)
                    {
                        await DeleteTask(task);
                    }
                    break;
            }
        }

        private async Task DeleteTask(UserTask task)
        {
            if (string.IsNullOrEmpty(_currentUserId) || string.IsNullOrEmpty(task.TaskId))
                return;

            try
            {
                await _firebaseClient
                    .Child("tasks")
                    .Child(_currentUserId)
                    .Child(task.TaskId)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete task: {ex.Message}", "OK");
            }
        }

        // Navigation methods
        private async void OnclickedShop(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Shop");
        }
        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) { }
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
}