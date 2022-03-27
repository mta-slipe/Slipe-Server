using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Lua;

struct LatentTransfer
{
    public ushort Id { get; set; }
    public Client Source { get; set; }
    public LatentEventCategory Category { get; set; }
    public List<byte> Data { get; set; } = new();

    public LatentTransfer(ushort id, Client source, LatentEventCategory category)
    {
        this.Id = id;
        this.Source = source;
        this.Category = category;
    }
}

public class LatentLuaEventPacketHandler : IPacketHandler<LatentLuaEventPacket>
{
    private readonly IElementRepository elementRepository;
    private readonly ILogger logger;
    private readonly MtaServer server;

    public PacketId PacketId => PacketId.PACKET_ID_LATENT_TRANSFER;
    private Dictionary<ushort, LatentTransfer> transfers;

    public LatentLuaEventPacketHandler(
        IElementRepository elementRepository,
        ILogger logger,
        MtaServer server
    )
    {
        this.elementRepository = elementRepository;
        this.logger = logger;
        this.server = server;

        this.transfers = new();
    }

    public void HandlePacket(Client client, LatentLuaEventPacket packet)
    {
        LatentTransfer transfer;
        switch (packet.Flag)
        {
            case LatentEventFlag.Head:
                if (packet.Header == null)
                    throw new Exception("Latent transfer Head flag encountered without corresponding header");

                transfer = new LatentTransfer(packet.Id, client, packet.Header.Value.Category);
                this.transfers[transfer.Id] = transfer;
                break;
            case LatentEventFlag.Cancel:
                this.transfers.Remove(packet.Id);
                return;
            default:
                transfer = this.transfers[packet.Id];
                break;

        }

        foreach (var b in packet.Data)
            transfer.Data.Add(b);

        if (packet.Flag == LatentEventFlag.Tail)
        {
            if (transfer.Category == LatentEventCategory.Packet)
            {
                var data = transfer.Data.ToArray();
                PacketId packetId = (PacketId)data[0];
                var length = BitConverter.ToUInt32(data, 1);
                this.transfers.Remove(packet.Id);

                this.server.EnqueuePacketToClient(client, packetId, data.Skip(5).ToArray());
            }
        }
    }
}
