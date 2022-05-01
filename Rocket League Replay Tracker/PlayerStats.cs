using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_League_Replay_Tracker
{
    internal struct PlayerStats
    {
        /// <summary>
        /// The player's name.
        /// </summary>
        public string name;
        /// <summary>
        /// The player's platform.
        /// </summary>
        public string platform;
        /// <summary>
        /// The name of the player's platform.
        /// </summary>
        public string platformName;
        /// <summary>
        /// The unique online ID of the player.
        /// This can be 0 (usually for bots or Epic accounts).
        /// </summary>
        public long onlineId;
        /// <summary>
        /// The ID of the team the player is on.
        /// </summary>
        public int team;
        /// <summary>
        /// The total score of the player.
        /// </summary>
        public int score;
        /// <summary>
        /// The total amount of goals of the player.
        /// </summary>
        public int goals;
        /// <summary>
        /// The total amount of assists of the player.
        /// </summary>
        public int assists;
        /// <summary>
        /// The total amount of saves of the player.
        /// </summary>
        public int saves;
        /// <summary>
        /// The total amount of shots of the player.
        /// </summary>
        public int shots;
        /// <summary>
        /// Is the player a bot?
        /// </summary>
        public byte isBot;
    }
}
