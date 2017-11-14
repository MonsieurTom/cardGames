using DotNetty.Codecs.Protobuf;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientCard
{
    class NettyNetworkController : INetworkController
    {
        private static bool _running = true;
        private static int _port = 0;
        private static String _address = "";
        private PlayerMsg _shellCmd = null;
        private IChannel _chan = null;

        public NettyNetworkController()
        {
            Console.WriteLine("---   Welcome to Jcoinche   ---");
            Console.WriteLine("Coinche game is a variant of the french card game Belotte.");

            try
            {
                Console.WriteLine("enter the host ip address : ");
                _address = System.Console.ReadLine();
                if (_address.Equals(""))
                    Environment.Exit(1);
                Console.WriteLine("enter a port : ");
                String str = System.Console.ReadLine();
                if (str.Equals(""))
                    Environment.Exit(1);
                _port = int.Parse(str);
                start().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Exception printed...... probably a bad argument.1");
                Thread.Sleep(5000);
            }
        }

        private static void changeState()
        {
            _running = false;
        }

        static async Task start()
        {
            var group = new MultithreadEventLoopGroup();

            try
            {
                Action action = changeState;
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast("framerDecoder", new ProtobufVarint32FrameDecoder());
                        pipeline.AddLast("decoder", new ProtobufDecoder(ServerMsg.Parser));
                        pipeline.AddLast("frameDecoder", new ProtobufVarint32LengthFieldPrepender());
                        pipeline.AddLast("encoder", new ProtobufEncoder());
                        pipeline.AddLast("handler", new NettyHandler(action));
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_address), _port));

                while (_running)
                {
                    try
                    {
                        string line = Console.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }
                        if (line != null)
                        {
                            PlayerMsg cmd = CoincheEncodeMessage.getInstance(action).encode(line);
                            if (cmd != null)
                            {
                                await bootstrapChannel.WriteAndFlushAsync(cmd);
                                cmd = null;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine("Exception printed..... closing program.....");
                        Thread.Sleep(5000);
                        _running = false;
                    }
                }

                await bootstrapChannel.CloseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Exception caught displayed, now closing the program.");
                Thread.Sleep(500000);
            }
            finally
            {
                group.ShutdownGracefullyAsync().Wait(1000);
            }
        }
    }
}
