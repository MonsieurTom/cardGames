using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server
{
    class CoincheGame
    {
        private String[] _cardNames = { "ace", "seven", "height", "nine", "ten", "jack", "queen", "king" };
        private String[] _trumps = { "none", "spade", "heart", "diamond", "club", "all" };
        private String[] _cardColors = { "spade", "heart", "diamond", "club" };

        private bool _leaved = false;
        private Deck _deck = new Deck();
        private ArrayList _players = new ArrayList();
        private int _playerTurn;
        private TRUMP _trump;
        private Action<MessageRequest> _sendMsg;
        public ArrayList msgFifo = new ArrayList();
        public readonly Mutex msgFifoLock = new Mutex();
        private ArrayList _stack = new ArrayList();
        private int _nbTurn;
        private int _firstPlayer;
        private int _dealValue;
        private Player _dealer;

        public CoincheGame(Player player1, Player player2, Player player3, Player player4, Action<MessageRequest> action)
        {
            try
            {
                setSendMsg(action);
                player1.setTeam(1);
                player2.setTeam(2);
                player3.setTeam(1);
                player4.setTeam(2);

                player1.setPlayerId(0);
                player2.setPlayerId(1);
                player3.setPlayerId(2);
                player4.setPlayerId(3);

                sendPlayerState(player1, PlayerState.Types.State.Playing);
                sendPlayerState(player2, PlayerState.Types.State.Playing);
                sendPlayerState(player3, PlayerState.Types.State.Playing);
                sendPlayerState(player4, PlayerState.Types.State.Playing);

                _players.Add(player1);
                _players.Add(player2);
                _players.Add(player3);
                _players.Add(player4);

                _playerTurn = 0;
                _nbTurn = 0;
                _firstPlayer = 0;
                _dealValue = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("exception printed, now exiting program!!!");
                Environment.Exit(1);
            }
        }

        public void start()
        {
            try
            {
                Thread thread = new Thread(run);
                thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("error thread Exception displayed!!!");
            }
        }

        public void run()
        {
            Console.WriteLine("startRun new game");
            try
            {
                distribute();

                int noDealTurn = 0;

                while (true)
                {
                    Player currentPlayer = (Player)_players[_playerTurn];
                    while (true)
                    {
                        sendPlayerState(currentPlayer, PlayerState.Types.State.Deal);
                        PlayerMsg playerMsg;
                        if ((playerMsg = waitFor(currentPlayer, PlayerMsg.Types.TypeMessage.Deal)) == null)
                            break;
                        Deal deal = playerMsg.Deal;
                        if (deal.Deal_ == -1)
                        {
                            sendActionValidation(currentPlayer, true);
                            sendPlayerDealed(currentPlayer, deal.Deal_, deal.Trump);
                            noDealTurn++;
                            break;
                        }
                        else if (deal.Deal_ > _dealValue)
                        {
                            noDealTurn = 0;
                            _dealValue = deal.Deal_;
                            _dealer = currentPlayer;
                            sendActionValidation(currentPlayer, true);
                            sendPlayerDealed(currentPlayer, deal.Deal_, deal.Trump);
                            break;
                        }
                        else
                            sendActionValidation(currentPlayer, false);
                    }
                    if (_leaved)
                        break;
                    if (noDealTurn == 3)
                        break;
                    _playerTurn++;
                    if (_playerTurn == 4)
                        _playerTurn = 0;
                }
                if (!_leaved)
                    sendDealWinner();
                _playerTurn = 0;
                _firstPlayer = 0;
                if (!_leaved)
                {
                    while (true)
                    {
                        Player player = (Player)_players[_playerTurn];
                        if (checkConnected())
                            break;
                        while (true)
                        {
                            if (checkConnected())
                                break;
                            sendPlayerState(player, PlayerState.Types.State.Yourturn);
                            PlayerMsg playerMsg;
                            if ((playerMsg = waitFor(player, PlayerMsg.Types.TypeMessage.Playcard)) == null)
                                break;
                            PlayCard playCardBuff = playerMsg.PlayCard;
                            Card card = DecodePlayedCard(playCardBuff);

                            if (validCard(card, player))
                            {
                                _stack.Add(card);
                                sendActionValidation(player.ClientId(), true);
                                sendPlayedCard(card, player);
                                break;
                            }
                            else
                                sendActionValidation(player.ClientId(), false);
                        }
                        if (checkConnected())
                            break;
                        _playerTurn++;
                        if (_playerTurn == 4)
                            _playerTurn = 0;
                        if (_stack.Count == 4)
                        {
                            _playerTurn = endStack();
                            _firstPlayer = _playerTurn;
                            _nbTurn++;
                        }
                        if (_nbTurn == 8)
                            break;
                    }
                }
                endGame();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Exception caught on table.");
            }
        }

        private bool checkConnected()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                if (tmp.Connected() == false)
                {
                    _leaved = true;
                    return (true);
                }
            }
            return (false);
        }

        public void setTrump(TRUMP trump)
        { _trump = trump; }

        private void endGame()
        {
            if (!_leaved)
            {
                Console.WriteLine("Ending a game and a thread.");
                Player tmp = (Player)_players[0];
                Player tmp2 = (Player)_players[2];
                int team1 = tmp.Score() + tmp2.Score();
                tmp = (Player)_players[1];
                tmp2 = (Player)_players[3];
                int team2 = tmp.Score() + tmp2.Score();

                ServerMsg msg = new ServerMsg
                {
                    TypeMessage = ServerMsg.Types.TypeMessage.Winner,
                    Winner = new GameWinner
                    {
                        Team1Score = team1,
                        Team2Score = team2,
                        DealerTeam = _dealer.Team(),
                        WinDeal = _dealValue <= TeamScore(_dealer.Team()),
                    }
                };
                for (int i = 0; i < _players.Count; i++)
                {
                    Player player = (Player)_players[i];
                    _sendMsg.Invoke(new MessageRequest(player.ClientId(), msg));
                }
            }
            else
            {
                for (int i = 0; i < _players.Count; i++)
                {
                    Player player = (Player)_players[i];
                    sendPlayerState(player, PlayerState.Types.State.Leave);
                }
            }
        }

        private int endStack()
        {
            int winnerPos = 0;
            bool winnerTrump = false;
            int winnerValue = 0;
            int total = 0;
            int tmpValue = 0;
            int pos = 0;
            Card tmp = (Card)_stack[0];
            CARD_COLOR stackColor = tmp.color;
            
            for (int i = 0; i < _stack.Count; i++)
            {
                tmp = (Card)_stack[i];
                tmpValue = CardValue(tmp);
                total += tmpValue;
                if (_trump != TRUMP.Tall && tmp.color != stackColor && !sameColor(_trump, tmp.color))
                {
                    ++pos;
                    continue;
                }
                if ((_trump == TRUMP.Tall && tmpValue > winnerValue) ||
                    (sameColor(_trump, tmp.color) && !winnerTrump) ||
                    (winnerTrump && sameColor(_trump, tmp.color) && tmpValue > winnerValue) |
                    tmpValue > winnerValue)
                {
                    winnerTrump = sameColor(_trump, tmp.color);
                    winnerPos = pos;
                    winnerValue = tmpValue;
                }
                ++pos;
            }
            Player player = (Player)_players[(_firstPlayer + winnerPos) % 4];
            player.addScore(total);
            sendStackWinner(player, total);
            _stack.Clear();

            return ((_firstPlayer + winnerPos) % 4);
        }

        public int CardValue(Card card)
        {
            switch(_trump)
            {
                case TRUMP.Tall:
                    switch (card.name)
                    {
                        case CARD_NAME.Ace:
                            return (7);
                        case CARD_NAME.Seven:
                            return (0);
                        case CARD_NAME.Height:
                            return (0);
                        case CARD_NAME.Nine:
                            return (9);
                        case CARD_NAME.Ten:
                            return (5);
                        case CARD_NAME.Jack:
                            return (14);
                        case CARD_NAME.Queen:
                            return (2);
                        case CARD_NAME.King:
                            return (3);
                    }
                    break;
                case TRUMP.Tnone:
                    switch (card.name)
                    {
                        case CARD_NAME.Ace:
                            return (19);
                        case CARD_NAME.Seven:
                            return (0);
                        case CARD_NAME.Height:
                            return (0);
                        case CARD_NAME.Nine:
                            return (0);
                        case CARD_NAME.Ten:
                            return (10);
                        case CARD_NAME.Jack:
                            return (2);
                        case CARD_NAME.Queen:
                            return (3);
                        case CARD_NAME.King:
                            return (4);
                    }
                    break;
                default:
                    switch (card.name)
                    {
                        case CARD_NAME.Ace:
                            return (sameColor(_trump, card.color) ? 11 : 11);
                        case CARD_NAME.Seven:
                            return (sameColor(_trump, card.color) ? 0 : 0);
                        case CARD_NAME.Height:
                            return (sameColor(_trump, card.color) ? 0 : 0);
                        case CARD_NAME.Nine:
                            return (sameColor(_trump, card.color) ? 14 : 0);
                        case CARD_NAME.Ten:
                            return (sameColor(_trump, card.color) ? 10 : 10);
                        case CARD_NAME.Jack:
                            return (sameColor(_trump, card.color) ? 20 : 2);
                        case CARD_NAME.Queen:
                            return (sameColor(_trump, card.color) ? 3 : 3);
                        case CARD_NAME.King:
                            return (sameColor(_trump, card.color) ? 4 : 4);
                    }
                    break;
            }
            return (0);
        }

        public bool sameColor(TRUMP trump, CARD_COLOR color)
        {
            if (trump == TRUMP.Tdiamond && color == CARD_COLOR.Diamond)
                return (true);
            if (trump == TRUMP.Thearth && color == CARD_COLOR.Hearth)
                return (true);
            if (trump == TRUMP.Tspade && color == CARD_COLOR.Spade)
                return (true);
            if (trump == TRUMP.Tclub && color == CARD_COLOR.Club)
                return (true);
            return (false);
        }

        private bool validCard(Card card, Player player)
        {
            return (player.haveCard(card));
        }
        private PlayerMsg waitFor(Player currentPlayer, PlayerMsg.Types.TypeMessage type)
        {
             while (true)
             {
                if (checkConnected())
                    return (null);
                msgFifoLock.WaitOne();
                if (msgFifo.Count > 0)
                {
                    ReceiveRequest msg = (ReceiveRequest)msgFifo[0];
                    msgFifo.Remove(msg);
                    if (msg._msg.TypeMessage == type && msg._clientId == currentPlayer.ClientId())
                    {
                        msgFifoLock.ReleaseMutex();
                        return (msg._msg);
                    }
                    else
                        sendActionValidation(msg._clientId, false);
                }
                msgFifoLock.ReleaseMutex();
                try
                {
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Console.WriteLine("exception caught kicking all player from this thread \n" + e.StackTrace + "\n error displayed");
                    for (int i = 0; i < _players.Count; i++)
                    {
                        Player tmp = (Player)_players[i];
                        sendPlayerState(tmp, PlayerState.Types.State.Leave);
                    }
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        private void distribute()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                for (int j = 0; j < 8; j++)
                {
                    Card newCard = _deck.RandCard();

                    sendDrawCard(tmp, newCard);
                    tmp.DrawCard(newCard);
                }
            }
        }

        private void sendDrawCard(Player player, Card card)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Drawcard,
                DrawCard = new DrawCard
                {
                    Color = card.color,
                    Name = card.name
                }
            };
            _sendMsg.Invoke(new MessageRequest(player.ClientId(), msg));
        }

        private void sendPlayerState(Player player, PlayerState.Types.State state)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Playerstate,
                PlayerState = new PlayerState
                {
                    Player = player.PlayerId(),
                    State = state,
                    Team = player.Team()
                }
            };
            _sendMsg.Invoke(new MessageRequest(player.ClientId(), msg));
        }

        private int TeamScore(int team)
        {
            int score = 0;

            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                if (tmp.Team() == team)
                    score += tmp.Score();
            }
            return (score);
        }

        private void sendActionValidation(int id, bool validation)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Actionvalidation,
                ActionValidation = new ActionValidation
                {
                    Accepted = validation
                }
            };
            _sendMsg.Invoke(new MessageRequest(id, msg));
        }

        private void sendActionValidation(Player player, bool validation)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Actionvalidation,
                ActionValidation = new ActionValidation
                {
                    Accepted = validation
                }
            };
            _sendMsg.Invoke(new MessageRequest(player.ClientId(), msg));
        }

        private void sendPlayedCard(Card card, Player player)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Playedcard,
                PlayedCard = new PlayedCard
                {
                    Color = card.color,
                    Name = card.name,
                    Player = player.PlayerId(),
                    Team = player.Team()
                }
            };
            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                _sendMsg.Invoke(new MessageRequest(tmp.PlayerId(), msg));
            }
        }

        private void sendPlayerDealed(Player player, int deal, TRUMP trump)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Playerdealed,
                PlayerDealed = new PlayerDealed
                {
                    Deal = deal,
                    Player = player.PlayerId(),
                    Team = player.Team(),
                    Trump = trump
                }
            };
            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                _sendMsg.Invoke(new MessageRequest(tmp.PlayerId(), msg));
            }
        }

        private void sendDealWinner()
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Dealwinner,
                DealWinner = new DealWinner
                {
                    Deal = _dealValue,
                    Player = _dealer.PlayerId(),
                    Team = _dealer.Team(),
                    Trump = _trump
                }
            };
            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                _sendMsg.Invoke(new MessageRequest(tmp.PlayerId(), msg));
            }
        }

        private void sendStackWinner(Player winner, int score)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Stackwinner,
                StackWinner = new StackWinner
                {
                    Player = winner.PlayerId(),
                    Team = winner.Team(),
                    Score = score
                }
            };
            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                _sendMsg.Invoke(new MessageRequest(tmp.PlayerId(), msg));
            }
        }

        public Card DecodePlayedCard(PlayCard card)
        {
            return (new Card(card.Color, card.Name));
        }

        public void setSendMsg(Action<MessageRequest> sendMsg)
        {
            _sendMsg = sendMsg;
        }

        public bool isInGame(int id)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                Player tmp = (Player)_players[i];
                if (tmp.ClientId() == id)
                    return (true);
            }
            return (false);
        }
    }
}
