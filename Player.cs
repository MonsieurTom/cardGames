using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class Player
    {
        private IClient _client;
        private int _team;
        private int _playerId;
        private int _deal;
        private int _score;
        private ArrayList _hand = new ArrayList();

        public Player(IClient client)
        {
            _client = client;
            _playerId = client.Id();
            _deal = -1;
            _team = -1;
            _score = 0;
        }

        public bool removeCard(Card card)
        {
            for (int i = 0; i < _hand.Count; i++)
            {
                Card Tmp = (Card)_hand[i];
                if (Tmp.color == card.color && Tmp.name == card.name)
                {
                    _hand.RemoveAt(i);
                    return (true);
                }
            }
            return (false);
        }

        public void DrawCard(Card card)
        { _hand.Add(card); }

        public bool haveCard(Card card)
        {
            for (int i = 0; i < _hand.Count; i++)
            {
                Card Tmp = (Card)_hand[i];
                if (Tmp.color == card.color && Tmp.name == card.name)
                    return (true);
            }
            return (false);
        }

        public void addScore(int score)
        { _score += score; }

        public void setPlayerId(int id)
        { _playerId = id; }

        public int Score()
        { return (_score); }

        public void setTeam(int team)
        { _team = team; }

        public int PlayerId()
        { return (_playerId); }

        public int ClientId()
        { return (_client.Id()); }

        public bool Connected()
        { return _client.Connected(); }

        public int Team()
        { return (_team); }

        public ArrayList Hand()
        { return (_hand); }

        public IClient  Client()
        { return (_client); }
    }
}
