using Firebase.Database;
using Firebase.Database.Query;
using gatchapon.Models;

namespace gatchapon
{
    public partial class TodoPage : ContentPage
    {
        private readonly FirebaseClient _firebaseClient = new("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");

        // This variable will hold the task we are editing.
        // If it's NULL, we are creating a new task.
        private UserTask _taskToEdit;
        private string _currentUserId;

        // Constructor for CREATING a new task
        public TodoPage()
        {
            InitializeComponent();
            _taskToEdit = null; // We are creating
        }

        // Constructor for EDITING an existing task
        public TodoPage(UserTask task)
        {
            InitializeComponent();
            _taskToEdit = task; // We are editing
        }

        // Load data and set up the UI when the page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _currentUserId = await SecureStorage.GetAsync("userId");

            if (_taskToEdit != null)
            {
                // --- EDIT MODE ---
                Title = "Edit Task";
                TitleLabel.Text = "Edit Your Task";
                TaskNameEntry.Text = _taskToEdit.TaskName;
                SaveButton.Text = "Update Task";

                // --- NEW CODE: Set the correct radio button ---
                if (_taskToEdit.Difficulty == "Medium")
                    MediumButton.IsChecked = true;
                else if (_taskToEdit.Difficulty == "Hard")
                    HardButton.IsChecked = true;
                else
                    EasyButton.IsChecked = true; // Default to Easy
            }
            else
            {
                // --- CREATE MODE ---
                Title = "Add New Task";
                TitleLabel.Text = "Create a New Task";
                SaveButton.Text = "Save Task";

                // --- NEW CODE: Default to Easy ---
                EasyButton.IsChecked = true;
            }
        }

        // This one button click now handles BOTH saving and updating
        private async void OnSaveTaskClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a task name.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_currentUserId))
            {
                await DisplayAlert("Error", "Could not find User ID. You may be logged out.", "OK");
                return;
            }

            // --- NEW LOGIC: Check difficulty and set reward ---
            string selectedDifficulty = "Easy";
            int selectedReward = 10; // Gold for Easy

            if (MediumButton.IsChecked)
            {
                selectedDifficulty = "Medium";
                selectedReward = 25; // Gold for Medium
            }
            else if (HardButton.IsChecked)
            {
                selectedDifficulty = "Hard";
                selectedReward = 50; // Gold for Hard
            }
            // --- END OF NEW LOGIC ---

            try
            {
                if (_taskToEdit != null)
                {
                    // --- UPDATE LOGIC ---
                    _taskToEdit.TaskName = TaskNameEntry.Text;
                    _taskToEdit.Difficulty = selectedDifficulty; // Add this
                    _taskToEdit.Reward = selectedReward;         // Add this

                    await _firebaseClient
                        .Child("tasks")
                        .Child(_currentUserId)
                        .Child(_taskToEdit.TaskId) // Use the EXISTING TaskId
                        .PutAsync(_taskToEdit);    // PutAsync UPDATES the data

                    await DisplayAlert("Success", "Task updated!", "OK");
                }
                else
                {
                    // --- CREATE LOGIC ---
                    var newTask = new UserTask
                    {
                        TaskName = TaskNameEntry.Text,
                        Streak = 0,
                        Total = 0,
                        IsCompletedToday = false,
                        UserId = _currentUserId,
                        Difficulty = selectedDifficulty, // Add this
                        Reward = selectedReward          // Add this
                    };

                    await _firebaseClient
                        .Child("tasks")
                        .Child(_currentUserId)
                        .PostAsync(newTask); // PostAsync CREATES new data

                    await DisplayAlert("Success", "Task saved!", "OK");
                }

                await Navigation.PopAsync(); // Go back to the dashboard
            }
            catch (Exception ex)
            {
                await DisplayAlert("Save Failed", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}