using SlipeServer.Scripting.Definitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;

namespace SlipeServer.Scripting.EventDefinitions;

public class AccountEventDefinitions(IMtaServer server, IAccountService accountService) : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<RootElement>(
            "onAccountCreate",
            (callback) =>
            {
                void callbackProxy(object? sender, AccountEventArgs e) => callback.CallbackDelegate(server.RootElement, e.Account);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => accountService.AccountCreated += callbackProxy,
                    Remove = (_) => accountService.AccountCreated -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onAccountRemove",
            (callback) =>
            {
                void callbackProxy(object? sender, AccountEventArgs e) => callback.CallbackDelegate(server.RootElement, e.Account);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => accountService.AccountRemoved += callbackProxy,
                    Remove = (_) => accountService.AccountRemoved -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<RootElement>(
            "onAccountDataChange",
            (callback) =>
            {
                void callbackProxy(object? sender, AccountDataChangedEventArgs e)
                    => callback.CallbackDelegate(server.RootElement, e.Account, e.Key, e.NewValue, e.OldValue);
                return new EventHandlerActions<RootElement>()
                {
                    Add = (_) => accountService.AccountDataChanged += callbackProxy,
                    Remove = (_) => accountService.AccountDataChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerLogin",
            (callback) =>
            {
                void callbackProxy(object? sender, PlayerLoggedInEventArgs e)
                    => callback.CallbackDelegate(e.Player, e.PreviousAccount, e.Account);
                return new EventHandlerActions<Player>()
                {
                    Add = (_) => accountService.PlayerLoggedIn += callbackProxy,
                    Remove = (_) => accountService.PlayerLoggedIn -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerLogout",
            (callback) =>
            {
                void callbackProxy(object? sender, PlayerLoggedOutEventArgs e)
                    => callback.CallbackDelegate(e.Player, e.PreviousAccount, e.GuestAccount);
                return new EventHandlerActions<Player>()
                {
                    Add = (_) => accountService.PlayerLoggedOut += callbackProxy,
                    Remove = (_) => accountService.PlayerLoggedOut -= callbackProxy
                };
            }
        );
    }
}
