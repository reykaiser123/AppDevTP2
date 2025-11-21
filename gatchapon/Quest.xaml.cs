using gatchapon.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gatchapon
{
    public partial class Quest : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private readonly FirebaseAuthService _authService = new();

        private string _currentUserId;
        private UserModel _currentUser;
        private DateTime _today;

        // Helper list to access UI elements
        private List<Tuple<Border, Image>> _weekFrames;

        public Quest()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _today = DateTime.Today; // Get today's date

            // Initialize UI references (Frames -> Borders)
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

        // --- NAVIGATION FIX ---
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async Task LoadAndDisplayUserData()
        {
            try
            {
                _currentUser = await _dbService.GetUserAsync<UserModel>(_currentUserId);

                if (_currentUser == null)
                {
                    await DisplayAlert("Error", "User data not found.", "OK");
                    return;
                }

                if (_currentUser.MonthlyCheckIns == null) _currentUser.MonthlyCheckIns = new List<string>();
                if (_currentUser.UnlockedCharacters == null) _currentUser.UnlockedCharacters = new List<string>();

                // 1. Update Daily Check-In Button
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

                // 2. Update Calendar
                UpdateWeekCalendarUI();

                // 3. Update Quests
                await UpdateQuestsUI();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load quests: {ex.Message}", "OK");
            }
        }

        private void UpdateWeekCalendarUI()
        {
            DateTime startOfWeek = _today.AddDays(-(int)_today.DayOfWeek);

            for (int i = 0; i < 7; i++)
            {
                DateTime day = startOfWeek.AddDays(i);
                string dayString = day.ToString("o");

                var (border, image) = _weekFrames[i];

                // Check using date part only to be safe
                bool isCheckedIn = _currentUser.MonthlyCheckIns.Any(d => d.StartsWith(day.ToString("yyyy-MM-dd")));

                if (isCheckedIn)
                {
                    border.BackgroundColor = Color.FromArgb("#4CAF50"); // Green
                    image.IsVisible = true;
                }
                else
                {
                    border.BackgroundColor = Colors.White;
                    image.IsVisible = false;
                }

                // Highlight Today
                if (day.Date == _today.Date)
                {
                    border.Stroke = new SolidColorBrush(Color.FromArgb("#A48E66"));
                    border.StrokeThickness = 2;
                }
                else
                {
                    border.Stroke = new SolidColorBrush(Colors.Transparent);
                    border.StrokeThickness = 0;
                }
            }
        }

        private async Task UpdateQuestsUI()
        {
            // --- Quest 1: 7-Day Streak ---
            // Reward increased to 500 Gold
            double streakProgress = Math.Min(_currentUser.CheckInStreak / 7.0, 1.0);
            StreakQuestProgress.Progress = streakProgress;
            StreakQuestLabel.Text = $"Check in for 7 days in a row. ({_currentUser.CheckInStreak} / 7)";

            if (_currentUser.CheckInStreak >= 7 && !_currentUser.Claimed7DayStreak)
            {
                StreakQuestButton.IsEnabled = true;
                StreakQuestButton.Text = "Claim (500 Gold)";
                StreakQuestButton.BackgroundColor = Color.FromArgb("#4CAF50");
            }
            else if (_currentUser.Claimed7DayStreak)
            {
                StreakQuestButton.IsEnabled = false;
                StreakQuestButton.Text = "Claimed";
                StreakQuestButton.BackgroundColor = Color.FromArgb("#8B7E74");
            }
            else
            {
                StreakQuestButton.IsEnabled = false;
                StreakQuestButton.Text = "Claim";
                StreakQuestButton.BackgroundColor = Color.FromArgb("#8B7E74");
            }

            // --- Quest 2: Monthly Check-in ---
            // Reward increased to 2000 Gold
            int daysInMonth = DateTime.DaysInMonth(_today.Year, _today.Month);
            int monthlyCheckins = _currentUser.MonthlyCheckIns.Count(d => DateTime.Parse(d).Month == _today.Month && DateTime.Parse(d).Year == _today.Year);

            double monthProgress = (double)monthlyCheckins / daysInMonth;
            MonthQuestProgress.Progress = monthProgress;
            MonthQuestLabel.Text = $"Check in every day for this month. ({monthlyCheckins} / {daysInMonth})";

            if (monthlyCheckins >= daysInMonth && !_currentUser.ClaimedMonthly)
            {
                MonthQuestButton.IsEnabled = true;
                MonthQuestButton.Text = "Claim (2000 Gold)";
                MonthQuestButton.BackgroundColor = Color.FromArgb("#4CAF50");
            }
            else if (_currentUser.ClaimedMonthly)
            {
                MonthQuestButton.IsEnabled = false;
                MonthQuestButton.Text = "Claimed";
                MonthQuestButton.BackgroundColor = Color.FromArgb("#8B7E74");
            }
            else
            {
                MonthQuestButton.IsEnabled = false;
                MonthQuestButton.Text = "Claim";
                MonthQuestButton.BackgroundColor = Color.FromArgb("#8B7E74");
            }

            // --- Quest 3: Complete 3 Tasks ---
            // DAILY RESET LOGIC: This ensures it repeats every day!
            string lastTaskDate = _currentUser.LastTaskCompletionDate;

            // If "LastTaskCompletionDate" is NOT today (meaning it's yesterday or older), reset everything.
            bool isNewDay = string.IsNullOrEmpty(lastTaskDate) || DateTime.Parse(lastTaskDate).Date != _today.Date;

            if (isNewDay)
            {
                // Reset the count to 0 for the new day
                _currentUser.TasksCompletedToday = 0;

                // Reset the "Claimed" status so they can claim it again today!
                _currentUser.Claimed3Tasks = false;

                // Set today as the new tracking date
                _currentUser.LastTaskCompletionDate = _today.ToString("o");

                // Save this reset state to Firebase so it persists
                await _dbService.SaveUserAsync(_currentUserId, _currentUser);
            }

            double tasksProgress = Math.Min(_currentUser.TasksCompletedToday / 3.0, 1.0);
            TasksQuestProgress.Progress = tasksProgress;
            TasksQuestLabel.Text = $"Complete 3 Daily Tasks today. ({_currentUser.TasksCompletedToday} / 3)";

            // Reward increased to 300 Gold
            if (_currentUser.TasksCompletedToday >= 3 && !_currentUser.Claimed3Tasks)
            {
                TasksQuestButton.IsEnabled = true;
                TasksQuestButton.Text = "Claim (300 Gold)";
                TasksQuestButton.BackgroundColor = Color.FromArgb("#4CAF50");
            }
            else if (_currentUser.Claimed3Tasks)
            {
                TasksQuestButton.IsEnabled = false;
                TasksQuestButton.Text = "Claimed";
                TasksQuestButton.BackgroundColor = Color.FromArgb("#8B7E74");
            }
            else
            {
                TasksQuestButton.IsEnabled = false;
                TasksQuestButton.Text = "Claim";
                TasksQuestButton.BackgroundColor = Color.FromArgb("#8B7E74");
            }
        }


        private async void OnCheckInClicked(object sender, EventArgs e)
        {
            CheckInButton.IsEnabled = false;

            string todayString = _today.ToString("o");

            bool isConsecutive = false;
            if (!string.IsNullOrEmpty(_currentUser.LastCheckInDate))
            {
                DateTime lastDate = DateTime.Parse(_currentUser.LastCheckInDate).Date;
                if (lastDate == _today.AddDays(-1).Date)
                {
                    isConsecutive = true;
                }
                else if (lastDate == _today.Date)
                {
                    return;
                }
            }

            if (isConsecutive)
            {
                _currentUser.CheckInStreak++;
            }
            else
            {
                _currentUser.CheckInStreak = 1;
            }

            _currentUser.LastCheckInDate = todayString;

            if (!_currentUser.MonthlyCheckIns.Contains(todayString))
            {
                _currentUser.MonthlyCheckIns.Add(todayString);
            }

            await _dbService.SaveUserAsync(_currentUserId, _currentUser);

            await LoadAndDisplayUserData();
            await DisplayAlert("Success", "Checked in!", "OK");
        }

        // --- Quest Claim Methods ---

        private async void OnClaimStreakQuestClicked(object sender, EventArgs e)
        {
            if (_currentUser.CheckInStreak < 7 || _currentUser.Claimed7DayStreak) return;

            _currentUser.Gold += 500; // UPDATED REWARD
            _currentUser.Claimed7DayStreak = true;

            // Optional: Reset streak so they can earn it again?
            // _currentUser.CheckInStreak = 0; 
            // _currentUser.Claimed7DayStreak = false;

            await _dbService.SaveUserAsync(_currentUserId, _currentUser);
            await DisplayAlert("Quest Complete!", "You earned 500 Gold!", "OK");
            await UpdateQuestsUI();
        }

        private async void OnClaimMonthQuestClicked(object sender, EventArgs e)
        {
            int daysInMonth = DateTime.DaysInMonth(_today.Year, _today.Month);
            int monthlyCheckins = _currentUser.MonthlyCheckIns.Count(d => DateTime.Parse(d).Month == _today.Month);

            if (monthlyCheckins < daysInMonth || _currentUser.ClaimedMonthly) return;

            _currentUser.Gold += 2000; // UPDATED REWARD
            _currentUser.ClaimedMonthly = true;

            await _dbService.SaveUserAsync(_currentUserId, _currentUser);
            await DisplayAlert("Quest Complete!", "You earned 2000 Gold!", "OK");
            await UpdateQuestsUI();
        }

        private async void OnClaimTasksQuestClicked(object sender, EventArgs e)
        {
            if (_currentUser.TasksCompletedToday < 3 || _currentUser.Claimed3Tasks) return;

            _currentUser.Gold += 300; // UPDATED REWARD
            _currentUser.Claimed3Tasks = true;

            await _dbService.SaveUserAsync(_currentUserId, _currentUser);
            await DisplayAlert("Quest Complete!", "You earned 300 Gold!", "OK");
            await UpdateQuestsUI();
        }
    }
}