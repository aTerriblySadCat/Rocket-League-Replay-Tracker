using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal struct PlayerStats
    {
        public string name;
        public string platform;
        public string platformName;
        public long onlineId;
        public int team;
        public int score;
        public int goals;
        public int assists;
        public int saves;
        public int shots;
        public byte isBot;
    }
}
