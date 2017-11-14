using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    interface IClient
    {
        void setReceiveMessageCallBack(Action<ReceiveRequest> action);
        void sendMessage(ServerMsg msg);
        int Id();
        bool Connected();
    }
}
