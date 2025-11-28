using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Models/AppModels.cs (NEW CENTRAL FILE)

namespace gatchapon.Models
{
    // Essential model for user authentication data
    // public class UserModel { ... } (This should already be in UserModel.cs)

    // Used in GachaBanner
    public class GachaItem
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public double Rate { get; set; }
        public string Rarity { get; set; }
        // Constructor, etc.
    }

    // Used in Characters.xaml.cs
    public class CharacterDefinition
    {
        public string Name { get; set; }
        public string Rarity { get; set; }
    }

    // Used in Characters.xaml.cs for UI binding
    public class CharacterDisplayItem
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public Color BorderColor { get; set; }
        public bool IsUnlocked { get; set; }
    }

    // Used in NotificationsPage (Friend Requests)
   

    // Used in ProfileSetting/FriendsPage (Friend List)
    public class FriendItem
    {
        public string Name { get; set; }
        public string FriendId { get; set; }
    }

    // Used in FriendsPage for complex display
    public class ContactItem
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Status { get; set; }
        public string TargetId { get; set; }
        public bool IsBot { get; set; }
    }
}
