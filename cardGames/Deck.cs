using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class Deck
    {
        private Random rand = new Random();
        private CARD_COLOR[] _colors = {CARD_COLOR.Spade, CARD_COLOR.Hearth, CARD_COLOR.Diamond, CARD_COLOR.Club};
        private CARD_NAME[] _names = {CARD_NAME.Ace, CARD_NAME.Seven, CARD_NAME.Height, CARD_NAME.Nine, CARD_NAME.Ten, CARD_NAME.Jack, CARD_NAME.Queen, CARD_NAME.King};
        private ArrayList _cards = new ArrayList();

        public Deck()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    this._cards.Add(new Card(_colors[i], _names[j]));
                }
            }
        }

        public Card RandCard()
        {
            int numCard;

            if (_cards.Count < 1)
                return (null);
            else
            {
                numCard = rand.Next(0, _cards.Count);

                Card newCard = (Card)_cards[numCard];
                _cards.RemoveAt(numCard);
                return (newCard);
            }
        }
    }
}
