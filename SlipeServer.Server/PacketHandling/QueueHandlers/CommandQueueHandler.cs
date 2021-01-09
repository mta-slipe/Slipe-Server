using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Definitions.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Server.Elements;
using Microsoft.Extensions.Logging;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class CommandQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_COMMAND };

        public CommandQueueHandler(ILogger logger, int sleepInterval, int workerCount) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            try
            {
                switch (queueEntry.PacketId)
                {
                    case PacketId.PACKET_ID_COMMAND:
                        CommandPacket commandPacket = new CommandPacket();
                        commandPacket.Read(queueEntry.Data);
                        queueEntry.Client.Player.TriggerCommand(commandPacket.Command, commandPacket.Arguments);
                        break;
                }
            } catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({queueEntry.PacketId}) failed.\n{e.Message}");
            }
        }

    }
}
