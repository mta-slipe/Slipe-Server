using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Scripting.EventDefinitions;

public class PlayerEventDefinitions : IEventDefinitions
{
    public void LoadInto(IScriptEventRuntime eventRuntime)
    {
        eventRuntime.RegisterEvent<Player>(
            "onPlayerWasted",
            (callback) =>
            {
                void callbackProxy(Element sender, PedWastedEventArgs e) => callback.CallbackDelegate(e.Source);
                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.Wasted += callbackProxy,
                    Remove = (element) => element.Wasted -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerChangeNick",
            (callback) =>
            {
                void callbackProxy(Element sender, ElementChangedEventArgs<string> e) => callback.CallbackDelegate(e.Source, e.OldValue, e.NewValue, false);
                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.NameChanged += callbackProxy,
                    Remove = (element) => element.NameChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerClick",
            (callback) =>
            {
                void callbackProxy(Element sender, PlayerCursorClickedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Button.ToString(), e.IsDown ? "down" : "up", e.Element, e.WorldPosition, e.Position);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.CursorClicked += callbackProxy,
                    Remove = (element) => element.CursorClicked -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerCommand",
            (callback) =>
            {
                void callbackProxy(Element sender, PlayerCommandEventArgs e)
                    => callback.CallbackDelegate(sender, e.Command);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.CommandEntered += callbackProxy,
                    Remove = (element) => element.CommandEntered -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerContact",
            (callback) =>
            {
                void callbackProxy(Player sender, ElementChangedEventArgs<Player, Element?> e)
                    => callback.CallbackDelegate(sender, e.OldValue, e.NewValue);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.ContactElementChanged += callbackProxy,
                    Remove = (element) => element.ContactElementChanged -= callbackProxy
                };
            }
        );

        eventRuntime.RegisterEvent<Player>(
            "onPlayerDamage",
            (callback) =>
            {
                void callbackProxy(Player sender, PlayerDamagedEventArgs e)
                    => callback.CallbackDelegate(sender, e.Damager, (int)e.WeaponType, (int)e.BodyPart, e.Loss);

                return new EventHandlerActions<Player>()
                {
                    Add = (element) => element.Damaged += callbackProxy,
                    Remove = (element) => element.Damaged -= callbackProxy
                };
            }
        );
    }
}
