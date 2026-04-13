using SlipeServer.Server;
using SlipeServer.Server.Bans;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Services;
using System.Collections.Generic;

namespace SlipeServer.Scripting.EventDefinitions;

public class ServerEventDefinitions(IMtaServer server, IBanService banService, IExplosionService explosionService) : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<RootElement>(
            "onShutdown",
            (callback) =>
            {
                void callbackProxy(IMtaServer _) => callback.CallbackDelegate(server.RootElement);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => server.Stopped += callbackProxy,
                    Remove = (_) => server.Stopped -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onBan",
            (callback) =>
            {
                void callbackProxy(object? sender, BanEventArgs e) => callback.CallbackDelegate(server.RootElement, e.Ban);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => banService.BanAdded += callbackProxy,
                    Remove = (_) => banService.BanAdded -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onUnban",
            (callback) =>
            {
                void callbackProxy(object? sender, BanEventArgs e) => callback.CallbackDelegate(server.RootElement, e.Ban);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => banService.BanRemoved += callbackProxy,
                    Remove = (_) => banService.BanRemoved -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onChatMessage",
            (callback) =>
            {
                var playerProxies = new Dictionary<Player, ElementEventHandler<Player, PlayerCommandEventArgs>>();

                void onPlayerJoin(Player player)
                {
                    void commandProxy(Player sender, PlayerCommandEventArgs e)
                    {
                        if (e.Command == "say")
                            callback.CallbackDelegate(player, string.Join(' ', e.Arguments), 0);
                    }
                    playerProxies[player] = commandProxy;
                    player.CommandEntered += commandProxy;
                }

                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) =>
                    {
                        foreach (var player in server.Players)
                            onPlayerJoin(player);
                        server.PlayerJoined += onPlayerJoin;
                    },
                    Remove = (_) =>
                    {
                        server.PlayerJoined -= onPlayerJoin;
                        foreach (var (player, proxy) in playerProxies)
                            player.CommandEntered -= proxy;
                        playerProxies.Clear();
                    }
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onExplosion",
            (callback) =>
            {
                var playerProxies = new Dictionary<Player, ElementEventHandler<Player, PlayerExplosionEventArgs>>();

                void onPlayerJoin(Player player)
                {
                    void explosionProxy(Player sender, PlayerExplosionEventArgs e)
                        => callback.CallbackDelegate(sender, e.Position.X, e.Position.Y, e.Position.Z, (int)e.ExplosionType);
                    playerProxies[player] = explosionProxy;
                    player.ExplosionCreated += explosionProxy;
                }

                void serviceExplosionProxy(object? sender, ExplosionEventArgs e)
                {
                    Element source = e.ResponsiblePlayer ?? (Element)server.RootElement;
                    callback.CallbackDelegate(source, e.Position.X, e.Position.Y, e.Position.Z, (int)e.Type);
                }

                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) =>
                    {
                        foreach (var player in server.Players)
                            onPlayerJoin(player);
                        server.PlayerJoined += onPlayerJoin;
                        explosionService.ExplosionCreated += serviceExplosionProxy;
                    },
                    Remove = (_) =>
                    {
                        server.PlayerJoined -= onPlayerJoin;
                        foreach (var (player, proxy) in playerProxies)
                            player.ExplosionCreated -= proxy;
                        playerProxies.Clear();
                        explosionService.ExplosionCreated -= serviceExplosionProxy;
                    }
                };
            }
        );
    }
}
