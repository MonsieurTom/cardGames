using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server
{
    class NettyHandler : SimpleChannelInboundHandler<PlayerMsg>
    {
        private static int _cptClients = 0;
        private ArrayList _clients = new ArrayList();
        private Action<IClient> _actAdd;
        private Action<IClient> _actDel;
        private Action<ReceiveRequest> _actRcv;

        public NettyHandler(Action<IClient> actAdd, Action<IClient> actDel, Action<ReceiveRequest> actRcv)
        {
            _actAdd = actAdd;
            _actDel = actDel;
            _actRcv = actRcv;
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            Console.WriteLine("---   New connection   ---");
            Console.WriteLine(context.Channel.RemoteAddress);
            IClient client = new NettyClient(context, _actRcv, _cptClients);
            _clients.Add((NettyClient)client);
            _actAdd.Invoke(client);
            _cptClients++;
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            Console.WriteLine("---   Deconnection   ---");
            for (int i = 0; i < _cptClients; i++)
            {
                NettyClient client = (NettyClient)_clients[i];
                if (client.Context() == context)
                {
                    Console.WriteLine(context.Channel.RemoteAddress);
                    client.setConnected(false);
                    _actDel(client);
                    break;
                }
            }
        }

        protected override void ChannelRead0(IChannelHandlerContext context, PlayerMsg msg)
        {
            Console.WriteLine("---   receiving a message   ---");
            for (int i = 0; i < _cptClients; i++)
            {
                NettyClient client = (NettyClient)_clients[i];
                if (client.Context() == context)
                {
                    ReceiveRequest rcv = new ReceiveRequest(client.Id(), msg);
                    Console.WriteLine("---   invoke rcv   ---");
                    client.CallBack().Invoke(rcv);
                    break;
                }
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            System.Console.WriteLine("Exception caught on client: " + context.Channel.RemoteAddress + ".");
            System.Console.WriteLine("Closing this client.");
            context.CloseAsync();
            context.WriteAndFlushAsync("Exception caught you'r being kick");
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            System.Console.WriteLine("Error on client : " + context.Channel.RemoteAddress + ".");
            context.WriteAndFlushAsync("You'r being kick!");
            context.CloseAsync();
        }
    }
}
