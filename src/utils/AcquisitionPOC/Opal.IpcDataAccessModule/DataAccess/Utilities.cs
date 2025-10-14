using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Polly;

namespace Thermofisher.Opal.IpcDataAccessModule.DataAccess
{
    public static class Utilities
    {
        public static CancellationTokenSource CreateCancellationTokenSource()
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            return cts;
        }

        public static ILoggerFactory GetLoggerFactory(LogLevel logLevel = LogLevel.Information)
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                //builder.AddDebug();
                builder.SetMinimumLevel(logLevel);
            });
        }
    }

    internal static class ConnectionProvider
    {
        internal static async Task<Channel> ConnectWithRetryAsync(
            string grpcServer,
            ChannelCredentials channelCredentials,
            ILogger logger = null,
            [CallerMemberName] string caller = ""
        )
        {
            var policy = Policy
                .Handle<Exception>() // broader catch to include failed manual checks
                .Or<RpcException>()
                .WaitAndRetryForeverAsync(
                    _ => TimeSpan.FromSeconds(2),
                    (ex, ts) =>
                        logger?.LogError(
                            $"[Retry] [{caller}] Connection failed: {ex.Message}. Retrying in {ts.TotalSeconds}s..."
                        )
                );

            Channel channel = null;

            await policy.ExecuteAsync(async () =>
            {
                channel = new Channel(
                    grpcServer,
                    ChannelCredentials.Insecure,
                    new[]
                    {
                        new ChannelOption(ChannelOptions.MaxReceiveMessageLength, 64 * 1024 * 1024),
                        new ChannelOption(ChannelOptions.MaxSendMessageLength, 64 * 1024 * 1024),
                    }
                );

                try
                {
                    await channel.ConnectAsync(deadline: DateTime.UtcNow.AddSeconds(2));
                }
                catch (Exception ex)
                {
                    throw new Exception("ConnectAsync failed: " + ex.Message, ex);
                }

                if (channel.State != ChannelState.Ready)
                {
                    throw new Exception("Channel is not in a ready state after ConnectAsync.");
                }
            });

            logger?.LogInformation("[Connected] gRPC channel is ready.");
            return channel;
        }
    }
}
