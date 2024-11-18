using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace SlipeServer.Server.Services;

class LatentTransfer
{
    public ushort Id { get; set; }
    public int Index { get; set; }
    public int Rate { get; set; } = 50000;
    public Player Player { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public PacketId PacketId { get; set; }
    public PacketPriority Priority { get; set; }
    public PacketReliability Reliability { get; set; }
    public ushort ResourceNetId { get; set; }

    public LatentTransfer(Player player)
    {
        this.Player = player;
    }

}

/// <summary>
/// Allows you to perform latent transfers to clients.
/// Latent transfers mean sending data (triggering lua events) while limiting the data rate.
/// This is useful when sending large amounts of data on limited bandwidth.
/// </summary>
public class LatentPacketService
{
    private readonly MtaServer server;
    private readonly RootElement root;
    private readonly HashSet<LatentTransfer> transfers;
    private readonly HashSet<Player> activeTransferPlayers;

    private readonly Timer sendTimer;
    private readonly uint bytesPerSend;
    private ushort index;

    public LatentPacketService(MtaServer server, RootElement root, Configuration configuration)
    {
        this.server = server;
        this.root = root;
        this.transfers = [];
        this.activeTransferPlayers = [];

        this.bytesPerSend = configuration.LatentBandwidthLimit / 1000 * configuration.LatentSendInterval;

        this.sendTimer = new Timer(configuration.LatentSendInterval)
        {
            AutoReset = false,
        };
        this.sendTimer.Start();
        this.sendTimer.Elapsed += (sender, args) => SendPackets();
    }

    private void SendPackets()
    {
        long bytesToSend = this.bytesPerSend;
        var transfers = new Queue<LatentTransfer>(this.transfers);

        while (bytesToSend > 0 && transfers.TryDequeue(out var transfer))
        {
            bytesToSend -= TrySendLatentPacket(transfer);
        }
        this.sendTimer.Start();
    }

    private int TrySendLatentPacket(LatentTransfer transfer)
    {
        if (this.activeTransferPlayers.Contains(transfer.Player) && transfer.Index == 0)
            return 0;

        var finalPosition = Math.Min(transfer.Index + transfer.Rate, transfer.Data.Length);
        var section = transfer.Data[transfer.Index..finalPosition];

        var packet = new LatentLuaEventPacket()
        {
            Id = transfer.Id,
            Data = section,
        };
        if (transfer.Index == 0)
        {
            EnrichPacketWithHeader(packet, transfer);
            this.activeTransferPlayers.Add(transfer.Player);

        } else if (finalPosition == transfer.Data.Length)
        {
            packet.Flag = LatentEventFlag.Tail;
            this.transfers.Remove(transfer);
            this.activeTransferPlayers.Remove(transfer.Player);
        }

        transfer.Index = finalPosition;
        packet.SendTo(transfer.Player);

        return transfer.Rate;
    }

    private void EnrichPacketWithHeader(LatentLuaEventPacket packet, LatentTransfer transfer)
    {
        packet.Flag = LatentEventFlag.Head;
        packet.Header = new LatentEventHeader()
        {
            FinalSize = (uint)transfer.Data.Length + sizeof(byte) + sizeof(uint),
            Category = LatentEventCategory.Packet,
            Rate = (uint)transfer.Rate,
            ResourceNetId = transfer.ResourceNetId
        };
        packet.Data = Array.Empty<byte>()
            .Concat([(byte)transfer.PacketId])
            .Concat(BitConverter.GetBytes((uint)transfer.Data.Length * 8))
            .Concat(packet.Data)
            .ToArray();
    }

    public void EnqueueLatentPacket(IEnumerable<Player> players, Packet packet, ushort resourceNetId, int rate = 50000)
    {
        EnqueueLatentPacket(players, packet.PacketId, packet.Write(), resourceNetId, rate, packet.Priority, packet.Reliability);
    }

    public void EnqueueLatentPacket(IEnumerable<Player> players, PacketId packetId, byte[] data, ushort resourceNetId, int rate = 50000, PacketPriority priority = PacketPriority.Medium, PacketReliability reliability = PacketReliability.Unreliable)
    {
        foreach (var player in players)
        {
            var transfer = new LatentTransfer(player)
            {
                Id = this.index,
                Data = data,
                PacketId = packetId,
                Priority = priority,
                Reliability = reliability,
                Rate = rate,
                ResourceNetId = resourceNetId
            };
            this.transfers.Add(transfer);
        }
        this.index = (ushort)((this.index + 1) % (1 << 15));
    }
}
