using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ClientCard
{
    class Hand
    {
        private bool _playedcard = false;
        private int _playedCardName = -1;
        private int _playedCardColor = -1;
        private static Hand instance;
        private ArrayList _cardName;
        private ArrayList _cardColor;

        private Hand()
        {
            _cardColor = new ArrayList();
            _cardName = new ArrayList();
        }

        public static Hand getInstance()
        {
            if (instance == null)
                instance = new Hand();
            return (instance);
        }

        public void addCard(String cardName, String CardColor)
        {
            _cardName.Add(cardName);
            _cardColor.Add(CardColor);
        }

        public String getCardName(int idx)
        {
            if (idx >= _cardName.Count)
                return (null);
            else
                return ((String)_cardName[idx]);
        }

        public String getCardColor(int idx)
        {
            if (idx >= _cardColor.Count)
                return (null);
            else
                return ((String)_cardColor[idx]);
        }

        public bool playCard(String cardName, String cardColor)
        {
            for (int i = 0; i < _cardColor.Count; i++)
            {
                String tmp = (String)_cardColor[i];
                String tmp2 = (String)_cardName[i];
           
                if (tmp.ToLower().Equals(cardColor.ToLower()) && tmp2.ToLower().Equals(cardName.ToLower()))
                {
                    _playedCardColor = i;
                    _playedCardName = i;
                    _playedcard = true;
                    return (true);
                }
            }
            return (false);
        }

        public bool deletePlayedCard()
        {
            if (_playedcard && _playedCardName != -1 && _playedCardColor != -1)
            {
                _cardName.RemoveAt(_playedCardName);
                _cardColor.RemoveAt(_playedCardColor);
                _playedCardName = -1;
                _playedCardColor = -1;
                _playedcard = false;
                return (true);
            }
            return (false);
        }
    }
}
