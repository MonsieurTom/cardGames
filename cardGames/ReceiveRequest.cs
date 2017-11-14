using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class ReceiveRequest
    {
        public int _clientId;
        public PlayerMsg _msg;

        public ReceiveRequest(int id, PlayerMsg msg)
        {
            _clientId = id;
            _msg = msg;
        }
    }
}
