using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ClientCard
{
    class NettyHandler : SimpleChannelInboundHandler<ServerMsg>
    {
        private Action _act; 

        public NettyHandler(Action action)
        {
            _act = action;
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            Console.WriteLine("---   connection Done   ---");

        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            Console.WriteLine("---   serveur deconnection   ---");
            Console.WriteLine("---   closing program   ---");
            Environment.Exit(1);
        }

        protected override void ChannelRead0(IChannelHandlerContext context, ServerMsg msg)
        {
            CoincheDecodeMessage.getInstance().decodeMessage(msg, _act);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            System.Console.WriteLine("Exception caught on client: " + context.Channel.RemoteAddress + ".");
            System.Console.WriteLine("Closing this client.");
            Thread.Sleep(5000);
            context.CloseAsync();
            _act.Invoke();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            System.Console.WriteLine("Error on client : " + context.Channel.RemoteAddress + ".");
            context.WriteAndFlushAsync("You'r being kick!");
            context.CloseAsync();
        }
    }
}
