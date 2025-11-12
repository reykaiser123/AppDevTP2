using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gatchapon.Models
{
    public class UserTask
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public int Streak { get; set; }
        public int Total { get; set; }
        public bool IsCompletedToday { get; set; }
        public string UserId { get; set; }
        public string Difficulty { get; set; } // "Easy", "Medium", "Hard"
        public int Reward { get; set; }       // How much gold it's worth
        public string LastUpdated { get; set; } // Tracks when the task was last completed

    }
}
