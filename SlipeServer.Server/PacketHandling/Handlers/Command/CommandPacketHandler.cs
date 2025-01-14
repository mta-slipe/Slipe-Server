using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.Command;

public class CommandPacketHandler : IPacketHandler<CommandPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_COMMAND;

    public void HandlePacket(IClient client, CommandPacket packet)
    {
        if(!string.IsNullOrWhiteSpace(packet.Command) && packet.Command.Length >= CommandPacket.MinCommandLength)
            client.Player.TriggerCommand(packet.Command, packet.Arguments);
    }
}
