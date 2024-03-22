using System.Collections.Generic;
using Gameboard.Objects.Buttons;

namespace Gameboard.CardsPracticalExample
{
    public class Hand
    {
        public string id;
        public HandType type;
        public List<string> cards;
        public List<string> currentlySelectedCards;

        public List<CompanionCardButton> buttons { get; set; }

        public Hand(string id)
        {
            this.id = id;
            cards = new List<string>();
            currentlySelectedCards = new List<string>();
        }
    }

    public enum HandType
    {
        Main,
        Resources,
        Hidden,
    }
}