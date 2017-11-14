using System;
using System.Collections.Generic;
using System.Text;
using DotNetty;
using DotNetty.Transport.Channels;

namespace Server
{
    class NettyClient : IClient
    {
        private int _id;
        private IChannelHandlerContext _context;
        private Action<ReceiveRequest> _actRcv;
        private bool _connected;

        public NettyClient(IChannelHandlerContext context, Action<ReceiveRequest> action, int id)
        {
            _id = id;
            _context = context;
            _actRcv = action;
            _connected = true;
        }

        public int  Id()
        {
            return (_id);
        }

        public IChannelHandlerContext   Context()
        {
            return (_context);
        }

        public void setConnected(bool state)
        {
            _connected = state;
        }

        public Action<ReceiveRequest>   CallBack()
        {
            return (_actRcv);
        }

        public void setReceiveMessageCallBack(Action<ReceiveRequest> action)
        {
            _actRcv = action;
        }

        public void sendMessage(ServerMsg msg)
        {
            _context.WriteAndFlushAsync(msg);
        }

        public bool Connected()
        { return (_connected); }

    }
}
