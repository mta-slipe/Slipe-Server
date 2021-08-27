using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.ConfigurationProviders.Configurations;
using SlipeServer.Lua;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Repositories;
using SlipeServer.Server.ServerOptions;
using System;
using System.IO;
using System.Threading;

namespace SlipeServer.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Program? program = null;
            try
            {
                program = new Program(args);
                program.Start();
            }
            catch (Exception exception)
            {
                if (program != null)
                {
                    program.Logger.LogCritical(exception, exception.Message);
                } else
                {
                    System.Console.WriteLine($"Error in startup {exception.Message}");
                }
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
            }
        }

        private readonly EventWaitHandle waitHandle = new(false, EventResetMode.AutoReset);
        private readonly MtaServer server;
        private readonly Configuration configuration;

        public ILogger Logger { get; }

        public Program(string[] args)
        {
            this.Logger = new ConsoleLogger();

            var configurationProvider = args.Length > 0 ? GetConfigurationProvider(args[0]) : null;

            this.configuration = configurationProvider?.GetConfiguration() ?? new Configuration()
            {
                IsVoiceEnabled = true
            };

            this.server = new MtaServer(
                (builder) =>
                {
                    builder.AddDefaults();

                    #if DEBUG
                        builder.AddNetWrapper(dllPath: "net_d", port: (ushort)(this.configuration.Port + 1));
                    #endif

                    builder.AddLogic<ServerTestLogic>();
                    builder.AddLogic<LuaTestLogic>();
                    builder.ConfigureServices(this.Configure);
                },
                this.configuration
            )
            {
                GameType = "Slipe Server",
                MapName = "N/A"
            };
            System.Console.CancelKeyPress += delegate
            {
                this.server.Stop();
                this.waitHandle.Set();
            };
        }

        public void Start()
        {
            this.server.Start();
            this.waitHandle.WaitOne();
        }

        private void Configure(ServiceCollection services)
        {
            services.AddSingleton<ILogger>(this.Logger);
            services.AddSingleton<ISyncHandlerMiddleware<ProjectileSyncPacket>, RangeSyncHandlerMiddleware<ProjectileSyncPacket>>(
                x => new RangeSyncHandlerMiddleware<ProjectileSyncPacket>(x.GetRequiredService<IElementRepository>(), this.configuration.ExplosionSyncDistance)
            );
            services.AddSingleton<ISyncHandlerMiddleware<DetonateSatchelsPacket>, RangeSyncHandlerMiddleware<DetonateSatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DetonateSatchelsPacket>(x.GetRequiredService<IElementRepository>(), this.configuration.ExplosionSyncDistance, false)
            );
            services.AddSingleton<ISyncHandlerMiddleware<DestroySatchelsPacket>, RangeSyncHandlerMiddleware<DestroySatchelsPacket>>(
                x => new RangeSyncHandlerMiddleware<DestroySatchelsPacket>(x.GetRequiredService<IElementRepository>(), this.configuration.ExplosionSyncDistance, false)
            );

            services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, SubscriptionSyncHandlerMiddleware<PlayerPureSyncPacket>>();
            services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, SubscriptionSyncHandlerMiddleware<KeySyncPacket>>();

            services.AddSingleton<ISyncHandlerMiddleware<LightSyncBehaviour>, MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>>(
                x => new MaxRangeSyncHandlerMiddleware<LightSyncBehaviour>(x.GetRequiredService<IElementRepository>(), this.configuration.LightSyncRange)
            );

            services.AddLua();
        }

        private IConfigurationProvider GetConfigurationProvider(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file {configPath} does not exist.");
            }

            string extension = Path.GetExtension(configPath);
            return extension switch
            {
                ".json" => new JsonConfigurationProvider(configPath),
                ".xml" => new XmlConfigurationProvider(configPath),
                ".toml" => new TomlConfigurationProvider(configPath),
                _ => throw new NotSupportedException($"Unsupported configuration extension {extension}"),
            };
        }
    }
}
