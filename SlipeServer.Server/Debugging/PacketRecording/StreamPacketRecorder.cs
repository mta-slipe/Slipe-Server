using SlipeServer.Packets.Enums;
using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace SlipeServer.Server.Debugging.PacketRecording;

public class StreamPacketRecorder : PacketRecorder
{
    private readonly Stream stream;
    private readonly HashSet<PacketId>? filterPackets;
    private int receivedBytes;
    private int sentBytes;

    public StreamPacketRecorder(Player player, MtaServer mtaServer, Stream stream, HashSet<PacketId>? excludePackets = null) : base(player, mtaServer)
    {
        this.stream = stream;
        this.filterPackets = excludePackets;
    }

    private void Write(string str)
    {
        var data = Encoding.UTF8.GetBytes(str);
        this.stream.Write(data);
    }

    protected override void HandlePacketSent(Packet packet, PacketDirection packetDirection)
    {
        if (this.filterPackets != null)
        {
            if (this.filterPackets.Contains(packet.PacketId))
                return;
        }

        var data = packet.Write();
        var rawData = Convert.ToBase64String(data);
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None
        };

        var serializedData = JsonConvert.SerializeObject(packet, settings);
        var direction = packetDirection == PacketDirection.Incoming ? "Incoming" : "Outgoing";
        switch (packetDirection)
        {
            case PacketDirection.Incoming:
                this.receivedBytes += data.Length;
                break;
            case PacketDirection.Outgoing:
                this.sentBytes += data.Length;
                break;
        }
        Write($"[{direction}] [At {DateTime.Now:HH-mm-ss.ffff}] {packet.PacketId} Data (len = {data.Length}) (base64: {rawData}) (serialized: {serializedData})\n");
    }

    public override void Dispose()
    {
        Write($"Total bytes sent: {this.sentBytes}\n");
        Write($"Total bytes received: {this.receivedBytes}\n");
        base.Dispose();
    }
}

