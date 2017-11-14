using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    interface INetworkController
    {
        void setNewClientCallBack(Action<IClient> actAdd);
        void setNewDelClientCallBack(Action<IClient> actDel);
        void setNewReceiveCallBack(Action<ReceiveRequest> actRcv);
    }
}
