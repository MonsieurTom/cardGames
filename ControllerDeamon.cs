using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class ControllerDeamon
    {
        private ArrayList _players = new ArrayList();
        private INetworkController networkController;
        private IGameController gameController;

        public ControllerDeamon()
        {
            Action<IClient> actDel = delClient;
            Action<IClient> actAdd = addClient;
            gameController = new CoincheController();
            Action<MessageRequest> actSnd = sndFct;
            Action<ReceiveRequest> actRcv = rcvFct;
            gameController.setSendMessageCallBack(actSnd);
            networkController = new NettyNetworkController(actAdd, actDel, actRcv);
        }

        public void sndFct(MessageRequest msg)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                IClient client = (IClient)_players[i];
                if (client.Id() == msg._clientId)
                    client.sendMessage(msg._msg);
            }
        }

        public void rcvFct(ReceiveRequest rcv)
        {

        }

        public void addClient(IClient client)
        {
            Console.WriteLine("---   new connection   ---");
            Action<ReceiveRequest> actRcv = gameController.receiveMsg;
            _players.Add(client);
            client.setReceiveMessageCallBack(actRcv);
            gameController.addPlayer(client);
        }

        public void delClient(IClient client)
        {
            Console.WriteLine("---   Deconnection   ---");
            _players.Remove(client);
            gameController.delPlayer(client);
        }
    }
}
