using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Services
{
    public class CommandService
    {
        private readonly Dictionary<string, HashSet<Command>> commandHandlers;

        public CommandService(MtaServer server)
        {
            this.commandHandlers = new();
            server.PlayerJoined += HandlePlayerJoin;
        }

        private void HandlePlayerJoin(Player player)
        {
            player.CommandEntered += HandlePlayerCommand;
        }

        private void HandlePlayerCommand(Player sender, PlayerCommandEventArgs args)
        {
            if (this.commandHandlers.ContainsKey(args.Command))
            {
                foreach (var handler in this.commandHandlers[args.Command])
                {
                    handler.Trigger(args.Source, args.Arguments);
                }
            }
        }

        public Command AddCommand(string command)
        {
            var handler = new Command(command);
            if (!this.commandHandlers.ContainsKey(command))
                this.commandHandlers[command] = new();

            this.commandHandlers[command].Add(handler);
            return handler;
        }

        public void RemoveCommand(Command command)
        {
            if (this.commandHandlers.ContainsKey(command.CommandText))
            {
                this.commandHandlers[command.CommandText].Remove(command);
                if (!this.commandHandlers[command.CommandText].Any())
                    this.commandHandlers.Remove(command.CommandText);
            }
        }
    }
}
