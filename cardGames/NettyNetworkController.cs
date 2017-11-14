using DotNetty.Codecs.Protobuf;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class NettyNetworkController : INetworkController
    {
        private static int _port = 4242;
        private static Action<IClient> _actAddClt;
        private static Action<IClient> _actDelClt;
        private static Action<ReceiveRequest> _actRcv;

        public NettyNetworkController(Action<IClient> actAdd, Action<IClient> actDel, Action<ReceiveRequest> actRcv)
        {
            setNewClientCallBack(actAdd);
            setNewDelClientCallBack(actDel);
            setNewReceiveCallBack(actRcv);
            Console.WriteLine("enter a port : ");
            String str = System.Console.ReadLine();

            try
            {
                _port = int.Parse(str);
            }
            catch
            {
                System.Console.WriteLine("Enter a propre Port.");
                System.Environment.Exit(0);
            }
            startServer().Wait();
        }

        public static async Task startServer()
        {
            IEventLoopGroup     boss = new MultithreadEventLoopGroup(1);
            IEventLoopGroup     worker = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(boss, worker)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast("framerDecoder", new ProtobufVarint32FrameDecoder());
                        pipeline.AddLast("decoder", new ProtobufDecoder(PlayerMsg.Parser));
                        pipeline.AddLast("frameDecoder", new ProtobufVarint32LengthFieldPrepender());
                        pipeline.AddLast("encoder", new ProtobufEncoder());
                        pipeline.AddLast("handler", new NettyHandler(_actAddClt, _actDelClt, _actRcv));
                    }));

                IChannel bootstrapChannel = await bootstrap.BindAsync(_port);

                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                System.Console.WriteLine("---   closing   ---");
                Task.WaitAll(boss.ShutdownGracefullyAsync(), worker.ShutdownGracefullyAsync());
            }
        }

        public void setNewClientCallBack(Action<IClient> actAdd)
        { _actAddClt = actAdd; }

        public void setNewDelClientCallBack(Action<IClient> actDel)
        { _actDelClt = actDel; }

        public void setNewReceiveCallBack(Action<ReceiveRequest> actRcv)
        { _actRcv = actRcv; }
    }
}
