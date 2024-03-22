using System.Collections.Generic;

namespace Gameboard.CardsPracticalExample
{
    public class PlayerCompanionData
    {
        public string userId { get; set; }
        public Dictionary<string, Hand> hands;
        public Hand activeHand { get; set; }
        public bool companionPlayer { get; set; }

        public PlayerCompanionData(string userId)
        {
            this.userId = userId;
            hands = new Dictionary<string, Hand>();
        }
    }
}