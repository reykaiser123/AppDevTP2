using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gatchapon.Models
{
    public class UserModel
    {
        public string UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        // --- ADD THIS MISSING PROPERTY ---
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        // This was also used in your ProfileSetting code, so ensure it's here too
        [JsonPropertyName("profilePictureUrl")]
        public string ProfilePictureUrl { get; set; }

        [JsonPropertyName("gold")]
        public int Gold { get; set; }

        [JsonPropertyName("gems")]
        public int Gems { get; set; }

        // ... Existing Quest/Shop properties ...
        public bool HasStreamGearCrate { get; set; }
        public bool HasSkyHighScarf { get; set; }
        public bool HasWovenCloudTapestry { get; set; }

        public List<string> UnlockedCharacters { get; set; } = new List<string>();

        public string LastCheckInDate { get; set; }
        public int CheckInStreak { get; set; }
        public List<string> MonthlyCheckIns { get; set; } = new List<string>();

        public string LastTaskCompletionDate { get; set; }
        public int TasksCompletedToday { get; set; }

        public bool Claimed7DayStreak { get; set; }
        public bool ClaimedMonthly { get; set; }
        public bool Claimed3Tasks { get; set; }
    }
}