using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class MessageRequest
    {
        public int _clientId;
        public ServerMsg _msg;

        public MessageRequest(int id, ServerMsg msg)
        {
            _clientId = id;
            _msg = msg;
        }
    }
}
