using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Lua;

struct LatentTransfer
{
    public ushort Id { get; set; }
    public IClient Source { get; set; }
    public LatentEventCategory Category { get; set; }
    public List<byte> Data { get; set; } = new();

    public LatentTransfer(ushort id, IClient source, LatentEventCategory category)
    {
        this.Id = id;
        this.Source = source;
        this.Category = category;
    }
}

public class LatentLuaEventPacketHandler : IPacketHandler<LatentLuaEventPacket>
{
    private readonly ILogger logger;
    private readonly MtaServer server;
    private readonly Dictionary<IClient, LatentTransfer> transfers;

    public PacketId PacketId => PacketId.PACKET_ID_LATENT_TRANSFER;

    public LatentLuaEventPacketHandler(
        ILogger logger,
        MtaServer server
    )
    {
        this.logger = logger;
        this.server = server;
        this.transfers = new();
    }

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

                this.server.EnqueuePacketToClient(client, packetId, data.Skip(5).ToArray());
            }
        }
    }
}
