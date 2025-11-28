using Firebase.Database;
using Firebase.Database.Query;
using gatchapon.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Collections.Generic; // Make sure this is included for Queue/List

namespace gatchapon
{
    // MODEL
    public class ChatMessage
    {
        public string Text { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public bool IsUser { get; set; }
        public string Timestamp { get; set; }
        public string DisplayImage { get; set; } // Holds the image URL/Path
    }

    [QueryProperty(nameof(TargetUserId), "targetId")]
    [QueryProperty(nameof(TargetUserName), "targetName")]
    public partial class ChatPage : ContentPage
    {
        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();
        private readonly BrainService _brainService = new BrainService();
        private readonly FirebaseClient _firebaseClient = new FirebaseClient("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");
        private readonly FirebaseDatabaseService _dbService = new FirebaseDatabaseService();

        public string TargetUserId { get; set; }
        public string TargetUserName { get; set; }

        private string _currentUserId;
        private string _currentUserName;

        // IMAGE VARIABLES
        private string _myProfilePic = "profile_placeholder.png";
        private string _targetProfilePic = "profile_placeholder.png";

        private string _chatRoomId;
        private bool _isHumanChat = false;
        private Queue<string> _storyScript = new Queue<string>();
        private bool _isStoryMode = false;

        public ChatPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _currentUserId = await SecureStorage.GetAsync("userId");
            _currentUserName = await SecureStorage.GetAsync("userName");

            // --- LOGIC TO DETERMINE CHARACTER NAME FIRST ---
            if (!string.IsNullOrEmpty(TargetUserId))
            {
                _isHumanChat = true;
                SetupHumanChat();
            }
            else
            {
                // If TargetUserName is NOT set (opened from floating Dashboard button)
                if (string.IsNullOrEmpty(TargetUserName))
                {
                    var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                    // Set TargetUserName to the Equipped Character, defaulting to "Marisol" if none is set.
                    TargetUserName = user?.EquippedCharacter ?? "Marisol";
                }

                _isHumanChat = false;
                // MUST AWAIT SETUPBOTCHAT to handle potential redirects/lockouts
                await SetupBotChat();
            }
            // --- END LOGIC TO DETERMINE CHARACTER NAME FIRST ---

            // 1. LOAD IMAGES NOW that TargetUserName is definitively set.
            await LoadProfileImages();
        }

        // --- UPDATED: LOAD IMAGES ---
        private async Task LoadProfileImages()
        {
            try
            {
                // Load My Image
                var myProfile = await _dbService.GetUserAsync<UserModel>(_currentUserId);
                if (myProfile != null && !string.IsNullOrEmpty(myProfile.ProfilePictureUrl))
                    _myProfilePic = myProfile.ProfilePictureUrl;

                // Load Target Image (If Human)
                if (!string.IsNullOrEmpty(TargetUserId))
                {
                    var targetProfile = await _dbService.GetUserAsync<UserModel>(TargetUserId);
                    if (targetProfile != null && !string.IsNullOrEmpty(targetProfile.ProfilePictureUrl))
                        _targetProfilePic = targetProfile.ProfilePictureUrl;
                }
                // Load Target Image (If Bot - uses TargetUserName which is now set)
                else
                {
                    _targetProfilePic = $"{TargetUserName.ToLower()}_char.png";
                }

                // Set Header Image after _targetProfilePic is defined
                if (_targetProfilePic.Contains("/") || _targetProfilePic.Contains("\\"))
                    HeaderFaceImage.Source = ImageSource.FromFile(_targetProfilePic);
                else
                    HeaderFaceImage.Source = _targetProfilePic;
            }
            catch { }
        }

        private void SetupHumanChat()
        {
            HeaderNameLabel.Text = TargetUserName;

            // Set Header Image logic is now in LoadProfileImages()

            var ids = new List<string> { _currentUserId, TargetUserId };
            ids.Sort();
            _chatRoomId = $"{ids[0]}_{ids[1]}";

            ListenToRealtimeMessages();
        }

        // --- FINAL SETUP BOT CHAT (Fixes Locked Access and Scripting) ---
        private async Task SetupBotChat()
        {
            HeaderNameLabel.Text = TargetUserName;

            // 1. Load user data for the unlock check
            var user = await _dbService.GetUserAsync<UserModel>(_currentUserId);
            var unlockedNames = user?.UnlockedCharacters ?? new List<string>();

            // 2. CHECK: If the target is NOT Marisol AND is NOT in the Unlocked list
            if (TargetUserName != "Marisol" && !unlockedNames.Contains(TargetUserName))
            {
                // Block chat access
                await DisplayAlert("LOCKED", $"You must unlock {TargetUserName} before chatting.", "OK");

                // Navigate away (e.g., back to dashboard)
                await Shell.Current.GoToAsync("//Dashboard");
                return; // Stop the method execution
            }

            // 3. Setup Chat (No story script, immediate AI mode)
            _isStoryMode = false;

            // Optional: Provide a single, immediate greeting based on the character
            if (TargetUserName == "Marisol")
            {
                ReceiveMessage("Hello! I'm here to help you stay organized. What's on your mind?");
            }
            else if (TargetUserName == "Maxine")
            {
                ReceiveMessage("Code check: green. Systems nominal. Ready for commands.");
            }
            else if (TargetUserName == "Roberto")
            {
                ReceiveMessage("Welcome! My kitchen is ready for your order. What culinary adventure awaits today?");
            }
        }
        // --- END FINAL SETUP BOT CHAT ---

        private void ListenToRealtimeMessages()
        {
            _firebaseClient
                .Child("chats")
                .Child(_chatRoomId)
                .AsObservable<ChatMessage>()
                .Subscribe(d =>
                {
                    if (d.Object != null)
                    {
                        var msg = d.Object;
                        msg.IsUser = (msg.SenderId == _currentUserId);

                        // ASSIGN IMAGE BASED ON SENDER
                        msg.DisplayImage = msg.IsUser ? _myProfilePic : _targetProfilePic;

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Messages.Add(msg);
                            ScrollToBottom();
                        });
                    }
                });
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputEntry.Text)) return;
            string text = InputEntry.Text;
            InputEntry.Text = string.Empty;

            // GET CURRENT TIME
            string timeNow = DateTime.Now.ToString("t");

            if (_isHumanChat)
            {
                var msg = new ChatMessage
                {
                    Text = text,
                    SenderId = _currentUserId,
                    SenderName = _currentUserName,
                    Timestamp = timeNow
                };

                await _firebaseClient
                    .Child("chats")
                    .Child(_chatRoomId)
                    .PostAsync(msg);
            }
            else
            {
                // Bot Logic
                Messages.Add(new ChatMessage
                {
                    Text = text,
                    IsUser = true,
                    Timestamp = timeNow,
                    DisplayImage = _myProfilePic
                });
                ScrollToBottom();

                if (_isStoryMode) await ProcessStoryProgress();
                else await ProcessAIResponse(text);
            }
        }
        private async Task ProcessStoryProgress()
        {
            await Task.Delay(1000);
            if (_storyScript.Count > 0) ReceiveMessage(_storyScript.Dequeue());
            else { _isStoryMode = false; ReceiveMessage("I'm ready to chat now!"); }
        }

        // ChatPage.xaml.cs - inside ProcessAIResponse

        private async Task ProcessAIResponse(string text)
        {
            // FIX 1: Pass the TargetUserName to the BrainService call
            string answer = await _brainService.GetAnswerAsync(text, TargetUserName);

            if (!string.IsNullOrEmpty(answer))
            {
                ReceiveMessage(answer);
            }
            else
            {
                bool teach = await DisplayAlert("Confused", "I don't know that. Teach me?", "Yes", "No");
                if (teach)
                {
                    string newAns = await DisplayPromptAsync("Teach", "What should I say?");
                    if (!string.IsNullOrEmpty(newAns))
                    {
                        // FIX 2: Pass the TargetUserName when teaching the AI
                        await _brainService.TeachAsync(text, newAns, TargetUserName);
                        ReceiveMessage("Thanks! I learned it.");
                    }
                }
            }
        }
        private void ReceiveMessage(string text, string time = "")
        {
            if (string.IsNullOrEmpty(time)) time = DateTime.Now.ToString("t");

            Messages.Add(new ChatMessage
            {
                Text = text,
                IsUser = false,
                Timestamp = time,
                DisplayImage = _targetProfilePic // Attach Bot/Friend Pic
            });
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (Messages.Count > 0) MessagesList.ScrollTo(Messages.Count - 1, position: ScrollToPosition.End, animate: true);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}