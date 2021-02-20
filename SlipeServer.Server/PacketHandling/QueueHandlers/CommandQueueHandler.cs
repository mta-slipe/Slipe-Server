using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Definitions.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Server.Elements;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class CommandQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_COMMAND };

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_COMMAND] = typeof(CommandPacket),
        };

        public CommandQueueHandler(ILogger logger, int sleepInterval, int workerCount) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
        }

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            {
                switch (packet)
                {
                    case CommandPacket commandPacket:
                        client.Player.TriggerCommand(commandPacket.Command, commandPacket.Arguments);
                        break;
                }
            } catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

    }
}
