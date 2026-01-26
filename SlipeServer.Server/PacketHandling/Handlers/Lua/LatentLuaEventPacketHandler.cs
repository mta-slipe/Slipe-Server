using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Clients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Lua;

struct LatentTransfer(ushort id, IClient source, LatentEventCategory category)
{
    public ushort Id { get; set; } = id;
    public IClient Source { get; set; } = source;
    public LatentEventCategory Category { get; set; } = category;
    public List<byte> Data { get; set; } = new();
}

public class LatentLuaEventPacketHandler(
    ILogger logger,
    IMtaServer server
    ) : IPacketHandler<LatentLuaEventPacket>
{
    private readonly Dictionary<IClient, LatentTransfer> transfers = new();

    public PacketId PacketId => PacketId.PACKET_ID_LATENT_TRANSFER;

    public void HandlePacket(IClient client, LatentLuaEventPacket packet)
    {
        LatentTransfer transfer;
        switch (packet.Flag)
        {
            case LatentEventFlag.Head:
                if (packet.Header == null)
                    throw new Exception("Latent transfer Head flag encountered without corresponding header");

                transfer = new LatentTransfer(packet.Id, client, packet.Header.Value.Category);
                this.transfers[client] = transfer;
                break;
            case LatentEventFlag.Cancel:
                this.transfers.Remove(client);
                return;
            default:
                transfer = this.transfers[client];
                break;
        }

        if (transfer.Id != packet.Id)
            throw new Exception("Latent transfer mismatch");

        foreach (var b in packet.Data)
            transfer.Data.Add(b);

        if (packet.Flag == LatentEventFlag.Tail)
        {
            if (transfer.Category == LatentEventCategory.Packet)
            {
                var data = transfer.Data.ToArray();
                PacketId packetId = (PacketId)data[0];
                var length = BitConverter.ToUInt32(data, 1);
                this.transfers.Remove(client);

                server.EnqueuePacketToClient(client, packetId, data.Skip(5).ToArray());
            }
        }
    }
}
