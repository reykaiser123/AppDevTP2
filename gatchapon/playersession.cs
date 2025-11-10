using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gatchapon
{
    public static class PlayerSession
    {
        public static PlayerData Player { get; set; } = new PlayerData { Coin = 100 };
    }
}
