using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Services;

class LatentTransfer(Player player)
{
    public ushort Id { get; set; }
    public int Index { get; set; }
    public int Rate { get; set; } = 50000;
    public Player Player { get; set; } = player;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public PacketId PacketId { get; set; }
    public PacketPriority Priority { get; set; }
    public PacketReliability Reliability { get; set; }
    public ushort ResourceNetId { get; set; }
}

/// <summary>
/// Allows you to perform latent transfers to clients.
/// Latent transfers mean sending data (triggering lua events) while limiting the data rate.
/// This is useful when sending large amounts of data on limited bandwidth.
/// </summary>
public class LatentPacketService : ILatentPacketService
{
    private readonly HashSet<LatentTransfer> transfers = [];
    private readonly HashSet<Player> activeTransferPlayers = [];

    private readonly uint bytesPerSend;
    private ushort index;

    public LatentPacketService(Configuration configuration, ITimerService timerService)
    {
        this.bytesPerSend = configuration.LatentBandwidthLimit / 1000 * configuration.LatentSendInterval;

        timerService.CreateTimer(SendPackets, TimeSpan.FromMilliseconds(configuration.LatentSendInterval));
    }

    private void SendPackets()
    {
        long bytesToSend = this.bytesPerSend;
        var transfers = new Queue<LatentTransfer>(this.transfers);

        while (bytesToSend > 0 && transfers.TryDequeue(out var transfer))
        {
            bytesToSend -= TrySendLatentPacket(transfer);
        }
    }

    private int TrySendLatentPacket(LatentTransfer transfer)
    {
        if (this.activeTransferPlayers.Contains(transfer.Player) && transfer.Index == 0)
            return 0;

        var rate = Math.Min(transfer.Rate, this.bytesPerSend);
        var finalPosition = (int)Math.Min(transfer.Index + rate, transfer.Data.Length);
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

        return section.Length;
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
            .Concat(new byte[] { (byte)transfer.PacketId })
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
