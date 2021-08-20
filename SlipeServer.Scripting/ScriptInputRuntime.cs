using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting
{
    public class ScriptInputRuntime : IScriptInputRuntime
    {
        private readonly List<RegisteredCommandHandler> registeredCommandHandlers;
        private readonly MtaServer server;

        public ScriptInputRuntime(MtaServer server)
        {
            this.registeredCommandHandlers = new List<RegisteredCommandHandler>();

            this.server = server;
            this.server.PlayerJoined += HandlePlayerJoined;
        }

        private void HandlePlayerJoined(Player player)
        {
            player.CommandEntered += CommandEntered;
            player.Destroyed += Destroyed;
        }

        private void Destroyed(Element obj)
        {
            (obj as Player).CommandEntered -= CommandEntered;
        }

        private void CommandEntered(Player player, PlayerCommandEventArgs e)
        {
            foreach (var commandHandler in this.registeredCommandHandlers)
            {
                if (commandHandler.CommandName == e.Command)
                {
                    commandHandler.Delegate.DynamicInvoke(player, e.Command, e.Arguments);
                }
            }
        }

        public void AddCommandHandler(string commandName, CommandDelegate callbackDelegate)
        {
            this.registeredCommandHandlers.Add(new RegisteredCommandHandler
            {
                CommandName = commandName,
                Delegate = callbackDelegate,
            });
        }

        public void RemoveCommandHandler(string commandName, CommandDelegate? callbackDelegate = null)
        {
            if (callbackDelegate == null)
                this.registeredCommandHandlers.RemoveAll(x => x.CommandName == commandName);
            else
                this.registeredCommandHandlers.RemoveAll(x =>
                {
                    var x1 = callbackDelegate.Target;
                    var y2 = x.Delegate.Target;
                    var foo = x1 == y2;
                    var asd = x.Delegate == callbackDelegate;
                    var asd2 = x.Delegate.Equals(callbackDelegate);
                    if(foo || asd || asd2)
                    {
                        var asdasd = 5;
                    }
                    return x.CommandName == commandName && asd;
                });
        }
    }

    internal struct RegisteredCommandHandler
    {
        public string CommandName { get; set; }
        public CommandDelegate Delegate { get; set; }
    }
}
