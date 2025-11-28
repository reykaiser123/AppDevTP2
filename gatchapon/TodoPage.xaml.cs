using gatchapon.Models;
using Microsoft.Maui.Storage;

namespace gatchapon
{
    public partial class TodoPage : ContentPage
    {
        private readonly FirebaseDatabaseService _dbService = new();
        private string _currentUserId;
        private UserTask _existingTask;

        public TodoPage()
        {
            InitializeComponent();
            LoadUser();
        }

        public TodoPage(UserTask taskToEdit)
        {
            InitializeComponent();
            _existingTask = taskToEdit;
            LoadUser();
            TitleLabel.Text = "EDIT TASK";
            SaveButton.Text = "Update Task";
            TaskNameEntry.Text = taskToEdit.TaskName;

            if (taskToEdit.Difficulty == "Easy") EasyButton.IsChecked = true;
            else if (taskToEdit.Difficulty == "Medium") MediumButton.IsChecked = true;
            else HardButton.IsChecked = true;
        }

        private async void LoadUser()
        {
            _currentUserId = await SecureStorage.GetAsync("userId");
        }

        private async void OnSaveTaskClicked(object sender, EventArgs e)
        {
            string taskName = TaskNameEntry.Text;

            if (string.IsNullOrWhiteSpace(taskName))
            {
                await DisplayAlert("Error", "Please enter a task name.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_currentUserId)) return;

            // --- UPDATED REWARD LOGIC ---
            string difficulty = "Easy";
            int reward = 50; // Easy = 50

            if (MediumButton.IsChecked)
            {
                difficulty = "Medium";
                reward = 100; // Medium = 100
            }
            else if (HardButton.IsChecked)
            {
                difficulty = "Hard";
                reward = 200; // Hard = 200
            }
            // ----------------------------

            var task = new UserTask
            {
                TaskName = taskName,
                Difficulty = difficulty,
                Reward = reward,
                IsCompletedToday = false,
                Streak = _existingTask?.Streak ?? 0,
                Total = _existingTask?.Total ?? 0,
                TaskId = _existingTask?.TaskId,
                UserId = _currentUserId,
                LastUpdated = _existingTask?.LastUpdated ?? ""
            };

            bool success = await _dbService.SaveUserTaskAsync(_currentUserId, task);

            if (success)
            {
                if (_existingTask != null)
                {
                    await DisplayAlert("Updated", "Task updated successfully.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    bool addAnother = await DisplayAlert("Task Saved!",
                        "Do you want to add another task right now?",
                        "Yes, Add Another",
                        "No, I'm Done");

                    if (addAnother)
                    {
                        TaskNameEntry.Text = string.Empty;
                        EasyButton.IsChecked = true;
                        TaskNameEntry.Focus();
                    }
                    else
                    {
                        await Navigation.PopAsync();
                    }
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to save task.", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}