using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Clients;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Lua;

struct LatentTransfer
{
    public ushort Id { get; set; }
    public IClient Source { get; set; }
    public LatentEventCategory Category { get; set; }
    public MemoryStream Data { get; set; }

    public LatentTransfer(ushort id, IClient source, LatentEventCategory category, uint capacity)
    {
        // If greater than 100MB
        if(capacity > 1024 * 1024 * 100)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        if(capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        this.Id = id;
        this.Source = source;
        this.Category = category;
        this.Data = new((int)capacity);
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
        if (packet.Header != null)
        {
            transfer = new LatentTransfer(packet.Id, client, packet.Header.Value.Category, packet.Header.Value.FinalSize);
            if(this.transfers.ContainsKey(client))
                throw new Exception("Client already have pending transfer");

            this.transfers[client] = transfer;
        } else
        {
            switch (packet.Flag)
            {
                case LatentEventFlag.Cancel:
                    this.transfers.Remove(client);
                    return;
                default:
                    transfer = this.transfers[client];
                    break;
            }
        }

        if (transfer.Id != packet.Id)
            throw new Exception("Latent transfer mismatch");

        transfer.Data.Write(packet.Data.ToArray());

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
