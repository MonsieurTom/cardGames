using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class CoincheController : IGameController
    {
        private ArrayList _players = new ArrayList();
        private ArrayList _games = new ArrayList();
        private Action<MessageRequest> _sendMsg;

        public void addPlayer(IClient client)
        {
            Player newPlayer = new Player(client);

            this.sendPlayerState(newPlayer, PlayerState.Types.State.Waitinggame);
            _players.Add(newPlayer);
            if (_players.Count == 4)
                launchGame();
        }

        public void delPlayer(IClient client)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                IClient thisClient = (IClient)_players[i];
                if (thisClient.Id() == client.Id())
                {
                    _players.Remove(client);
                    break;
                }
            }
        }

        public void setSendMessageCallBack(Action<MessageRequest> sendMessage)
        {
            _sendMsg = sendMessage;
        }

        public void launchGame()
        {
            Console.WriteLine("---   launchGame   ---");
            Player player1 = (Player)_players[0];
            Player player2 = (Player)_players[1];
            Player player3 = (Player)_players[2];
            Player player4 = (Player)_players[3];

            _players.Remove(player1);
            _players.Remove(player2);
            _players.Remove(player3);
            _players.Remove(player4);

            CoincheGame game = new CoincheGame(player1, player2, player3, player4, _sendMsg);

            _games.Add(game);
            Console.WriteLine("---   Calling start method of coincheGame   ---");
            game.start();
        }

        private void sendPlayerState(Player player, PlayerState.Types.State state)
        {
            ServerMsg msg = new ServerMsg
            {
                TypeMessage = ServerMsg.Types.TypeMessage.Playerstate,
                PlayerState = new PlayerState
                {
                    Player = player.ClientId(),
                    State = state,
                    Team = player.Team()
                }
            };

            _sendMsg.Invoke(new MessageRequest(player.ClientId(), msg));
        }

        public void receiveMsg(ReceiveRequest msg)
        {
            for (int i = 0; i < _games.Count; i++)
            {
                CoincheGame game = (CoincheGame)_games[i];
                if (game.isInGame(msg._clientId))
                {
                    game.msgFifoLock.WaitOne();
                    game.msgFifo.Add(msg);
                    Console.WriteLine("---   message added in fifo   ---");
                    game.msgFifoLock.ReleaseMutex();
                }
            }
        }
    }
}
