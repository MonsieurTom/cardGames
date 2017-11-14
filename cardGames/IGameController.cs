using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    interface IGameController
    {
        void addPlayer(IClient client);
        void receiveMsg(ReceiveRequest msg);
        void setSendMessageCallBack(Action<MessageRequest> sendMessage);
        void delPlayer(IClient client);
    }
}
