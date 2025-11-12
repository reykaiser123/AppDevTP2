using gatchapon.Models;
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Firebase.Database;
using Firebase.Database.Query;

namespace gatchapon
{
    public partial class Quest : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new();
        private readonly FirebaseClient _firebaseClient = new("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");

        private string _currentUserId;
        private UserModel _currentUser;
        private DateTime _today;

        // --- THIS IS THE FIRST CHANGE ---
        // A helper list to access the UI elements easily
        private List<Tuple<Border, Image>> _weekFrames;

        public Quest()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _today = DateTime.Today; // Get today's date

            // --- THIS IS THE SECOND CHANGE (Frame -> Border) ---
            _weekFrames = new List<Tuple<Border, Image>>
            {
                new(SunFrame, SunImage), new(MonFrame, MonImage), new(TueFrame, TueImage),
                new(WedFrame, WedImage), new(ThuFrame, ThuImage), new(FriFrame, FriImage),
                new(SatFrame, SatImage)
            };

            _currentUserId = await SecureStorage.GetAsync("userId");
            if (string.IsNullOrEmpty(_currentUserId))
            {
                await DisplayAlert("Error", "User not logged in.", "OK");
                return;
            }

            await LoadAndDisplayUserData();
        }

        private async Task LoadAndDisplayUserData()
        {
            _currentUser = await _dbService.GetUserAsync<UserModel>(_currentUserId);
            if (_currentUser == null)
            {
                _currentUser = new UserModel(); // Create new user data if it doesn't exist
            }

            // 1. Update the Daily Check-In Button
            string todayString = _today.ToString("o");
            if (_currentUser.LastCheckInDate == todayString)
            {
                CheckInButton.Text = "Checked in for Today!";
                CheckInButton.IsEnabled = false;
                CheckInButton.BackgroundColor = Color.FromArgb("#4CAF50"); // Green
            }
            else
            {
                CheckInButton.Text = "Check-In for Today";
                CheckInButton.IsEnabled = true;
                CheckInButton.BackgroundColor = Color.FromArgb("#A48E66"); // Brown
            }

            // 2. Update the 7-Day Calendar Visual
            UpdateWeekCalendarUI();

            // 3. Update the Quests Progress
            await UpdateQuestsUI();
        }

        private void UpdateWeekCalendarUI()
        {
            DateTime startOfWeek = _today.AddDays(-(int)_today.DayOfWeek);

            for (int i = 0; i < 7; i++)
            {
                DateTime day = startOfWeek.AddDays(i);
                string dayString = day.ToString("o");

                var (border, image) = _weekFrames[i];

                if (_currentUser.MonthlyCheckIns.Contains(dayString))
                {
                    border.BackgroundColor = Color.FromArgb("#4CAF50"); // Green
                    image.IsVisible = true;
                }
                else
                {
                    border.BackgroundColor = Colors.White;
                    image.IsVisible = false;
                }

                // --- THIS IS THE THIRD CHANGE (BorderColor -> Stroke) ---
                // Highlight today's date
                if (day == _today)
                {
                    border.Stroke = new SolidColorBrush(Color.FromArgb("#A48E66")); // Use Stroke
                    border.StrokeThickness = 2;
                }
                else
                {
                    border.Stroke = new SolidColorBrush(Colors.Transparent); // Use Stroke
                    border.StrokeThickness = 0;
                }
            }
        }

        // This method checks the "Complete 3 Tasks" quest
        private async Task UpdateQuestsUI()
        {
            // --- Quest 1: 7-Day Streak ---
            double streakProgress = _currentUser.CheckInStreak / 7.0;
            StreakQuestProgress.Progress = streakProgress;
            StreakQuestLabel.Text = $"Check in for 7 days in a row. ({_currentUser.CheckInStreak} / 7)";
            if (streakProgress >= 1 && !_currentUser.Claimed7DayStreak)
            {
                StreakQuestButton.IsEnabled = true;
                StreakQuestButton.Text = "Claim (100 Gold)";
            }
            else if (_currentUser.Claimed7DayStreak)
            {
                StreakQuestButton.IsEnabled = false;
                StreakQuestButton.Text = "Claimed";
            }

            // --- Quest 2: Monthly Check-in ---
            int daysInMonth = DateTime.DaysInMonth(_today.Year, _today.Month);
            int monthlyCheckins = _currentUser.MonthlyCheckIns.Count(d => DateTime.Parse(d).Month == _today.Month);
            double monthProgress = (double)monthlyCheckins / daysInMonth;
            MonthQuestProgress.Progress = monthProgress;
            MonthQuestLabel.Text = $"Check in every day for this month. ({monthlyCheckins} / {daysInMonth})";
            if (monthProgress >= 1 && !_currentUser.ClaimedMonthly)
            {
                MonthQuestButton.IsEnabled = true;
                MonthQuestButton.Text = "Claim (500 Gold)";
            }
            else if (_currentUser.ClaimedMonthly)
            {
                MonthQuestButton.IsEnabled = false;
                MonthQuestButton.Text = "Claimed";
            }

            // --- Quest 3: Complete 3 Tasks ---
            // Reset count if it's a new day
            if (_currentUser.LastTaskCompletionDate != _today.ToString("o"))
            {
                _currentUser.TasksCompletedToday = 0;
            }

            double tasksProgress = _currentUser.TasksCompletedToday / 3.0;
            TasksQuestProgress.Progress = tasksProgress;
            TasksQuestLabel.Text = $"Complete 3 Daily Tasks today. ({_currentUser.TasksCompletedToday} / 3)";
            if (tasksProgress >= 1 && !_currentUser.Claimed3Tasks)
            {
                TasksQuestButton.IsEnabled = true;
                TasksQuestButton.Text = "Claim (50 Gold)";
            }
            else if (_currentUser.Claimed3Tasks)
            {
                TasksQuestButton.IsEnabled = false;
                TasksQuestButton.Text = "Claimed";
            }
        }


        private async void OnCheckInClicked(object sender, EventArgs e)
        {
            CheckInButton.IsEnabled = false;

            string todayString = _today.ToString("o");
            string yesterdayString = _today.AddDays(-1).ToString("o");

            // 1. Update Streak
            if (_currentUser.LastCheckInDate == yesterdayString)
            {
                _currentUser.CheckInStreak++;
            }
            else if (_currentUser.LastCheckInDate != todayString)
            {
                _currentUser.CheckInStreak = 1;
            }

            // 2. Update Check-In Dates
            _currentUser.LastCheckInDate = todayString;
            if (!_currentUser.MonthlyCheckIns.Contains(todayString))
            {
                _currentUser.MonthlyCheckIns.Add(todayString);
            }

            // 3. Save to Firebase
            await _dbService.SaveUserAsync(_currentUserId, _currentUser);

            // 4. Refresh all UI
            await LoadAndDisplayUserData();
        }

        // --- Quest Claim Methods ---

        private async void OnClaimStreakQuestClicked(object sender, EventArgs e)
        {
            if (_currentUser.CheckInStreak < 7 || _currentUser.Claimed7DayStreak) return;

            _currentUser.Gold += 100; // Give 100 Gold
            _currentUser.Claimed7DayStreak = true;

            await _dbService.SaveUserAsync(_currentUserId, _currentUser);
            await DisplayAlert("Quest Complete!", "You earned 100 Gold!", "OK");
            await UpdateQuestsUI(); // Refresh UI
        }

        private async void OnClaimMonthQuestClicked(object sender, EventArgs e)
        {
            int daysInMonth = DateTime.DaysInMonth(_today.Year, _today.Month);
            int monthlyCheckins = _currentUser.MonthlyCheckIns.Count(d => DateTime.Parse(d).Month == _today.Month);
            if (monthlyCheckins < daysInMonth || _currentUser.ClaimedMonthly) return;

            _currentUser.Gold += 500; // Give 500 Gold
            _currentUser.ClaimedMonthly = true;

            await _dbService.SaveUserAsync(_currentUserId, _currentUser);
            await DisplayAlert("Quest Complete!", "You earned 500 Gold!", "OK");
            await UpdateQuestsUI();
        }

        private async void OnClaimTasksQuestClicked(object sender, EventArgs e)
        {
            if (_currentUser.TasksCompletedToday < 3 || _currentUser.Claimed3Tasks) return;

            _currentUser.Gold += 50; // Give 50 Gold
            _currentUser.Claimed3Tasks = true;

            await _dbService.SaveUserAsync(_currentUserId, _currentUser);
            await DisplayAlert("Quest Complete!", "You earned 50 Gold!", "OK");
            await UpdateQuestsUI();
        }
    }
}