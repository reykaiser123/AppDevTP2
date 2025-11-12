using System.Text.Json.Serialization;

namespace gatchapon.Models
{
    public class UserModel
    {
        public string UserId { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        public string ProfilePictureUrl { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("gold")]
        public int Gold { get; set; }
        [JsonPropertyName("gems")]
        public int Gems { get; set; }

        // --- ADD ALL OF THESE NEW PROPERTIES ---

        // For Daily Check-In
        public string LastCheckInDate { get; set; } // Stores the date of the last check-in
        public int CheckInStreak { get; set; }     // For the "7 days in a row" quest

        // For "Check in every day this month" quest
        public List<string> MonthlyCheckIns { get; set; } = new List<string>();

        // For "Complete 3 Daily Tasks" quest
        public string LastTaskCompletionDate { get; set; } // Tracks the day
        public int TasksCompletedToday { get; set; }      // Tracks the count

        // To track which quests have been claimed
        public bool Claimed7DayStreak { get; set; }
        public bool ClaimedMonthly { get; set; }
        public bool Claimed3Tasks { get; set; }
    }
}