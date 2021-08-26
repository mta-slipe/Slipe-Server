using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Server.PacketHandling.Handlers.Command
{
    public class CommandPacketHandler : IPacketHandler<CommandPacket>
    {
        public PacketId PacketId => PacketId.PACKET_ID_COMMAND;

        public void HandlePacket(Client client, CommandPacket packet)
        {
            client.Player.TriggerCommand(packet.Command, packet.Arguments);
        }
    }
}
