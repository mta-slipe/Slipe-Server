using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Events;
using SlipeServer.Server.Events.Attributes;
using SlipeServer.Server.Events.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Console
{
    public class SlipeEventHandler : ILuaEventHandler
    {
        private readonly ILogger logger;

        public string BaseName => "Slipe.";

        public SlipeEventHandler(ILogger logger)
        {
            this.logger = logger;
        }

        [EventName("TestA")]
        public void TestA(LuaEvent value)
        {
            this.logger.LogInformation($"SlipeEventHandler A: {value.Name}");
        }
        [EventName("TestB")]
        public void TestB(LuaEvent value)
        {
            this.logger.LogInformation($"SlipeEventHandler B: {value.Name}");
        }
    }
}
