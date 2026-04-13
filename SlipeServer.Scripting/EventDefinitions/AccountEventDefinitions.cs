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
    }
}
