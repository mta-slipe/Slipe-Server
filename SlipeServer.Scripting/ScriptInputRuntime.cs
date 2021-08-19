using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
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
        private readonly IElementRepository elementRepository;

        public ScriptInputRuntime(MtaServer server, IElementRepository elementRepository)
        {
            this.registeredCommandHandlers = new List<RegisteredCommandHandler>();

            this.server = server;
            this.elementRepository = elementRepository;
            //this.server.ElementCreated += HandleElementCreation;
        }

        public void AddCommandHandler(string commandName, CommandDelegate callbackDelegate)
        {
            registeredCommandHandlers.Add(new RegisteredCommandHandler
            {
                CommandName = commandName,
                Delegate = callbackDelegate,
            });
        }
    }

    internal struct RegisteredCommandHandler
    {
        public string CommandName { get; set; }
        public CommandDelegate Delegate { get; set; }
    }
}
