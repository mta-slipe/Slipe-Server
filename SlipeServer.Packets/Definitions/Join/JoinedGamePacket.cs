using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;

namespace SlipeServer.Packets.Definitions.Join;

public class JoinedGamePacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_SERVER_JOINEDGAME;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ElementId ClientId { get; }
    public int PlayerCount { get; }
    public ElementId RootId { get; }
    public HttpDownloadType HttpDownloadType { get; }
    public ushort HttpPort { get; }
    public string HttpUrl { get; } = string.Empty;
    public int HttpConnectionsPerClient { get; }
    public bool IsFakeLagCommandEnabled { get; }
    public int VoiceSampleRate { get; }
    public int VoiceBitRate { get; }
    public int EnableClientChecks { get; }
    public bool IsVoiceEnabled { get; }
    public int VoiceQuality { get; }

    public JoinedGamePacket()
    {

    }

    public JoinedGamePacket(
        ElementId clientId,
        int playerCount,
        ElementId rootId,
        HttpDownloadType httpDownloadType,
        ushort httpPort,
        string httpUrl,
        int httpConnectionsPerClient,
        int enableClientChecks,
        bool isFakeLagCommandEnabled = false,
        bool isVoiceEnabled = true,
        int voiceSampleRate = 2,
        int voiceQuality = 4,
        int voiceBitRate = 24600)
    {
        this.ClientId = clientId;
        this.PlayerCount = playerCount;
        this.RootId = rootId;
        this.HttpDownloadType = httpDownloadType;
        this.HttpPort = httpPort;
        this.HttpUrl = httpUrl;
        this.HttpConnectionsPerClient = httpConnectionsPerClient;
        this.EnableClientChecks = enableClientChecks;
        this.IsFakeLagCommandEnabled = isFakeLagCommandEnabled;
        this.IsVoiceEnabled = isVoiceEnabled;
        this.VoiceSampleRate = voiceSampleRate;
        this.VoiceQuality = voiceQuality;
        this.VoiceBitRate = voiceBitRate;
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.ClientId);
        builder.Write((byte)this.PlayerCount);
        builder.Write(this.RootId);

        builder.Write(this.EnableClientChecks);

        builder.Write(this.IsVoiceEnabled);
        builder.WriteCapped(this.VoiceSampleRate, 2);
        builder.WriteCapped(this.VoiceQuality, 4);
        builder.WriteCompressed((uint)this.VoiceBitRate);

        builder.Write(this.IsFakeLagCommandEnabled);

        builder.Write(this.HttpConnectionsPerClient);
        builder.Write((byte)this.HttpDownloadType);

        switch (this.HttpDownloadType)
        {
            case HttpDownloadType.HTTP_DOWNLOAD_ENABLED_PORT:
                builder.Write(this.HttpPort);
                break;

            case HttpDownloadType.HTTP_DOWNLOAD_ENABLED_URL:
                builder.Write(this.HttpPort);
                builder.Write(this.HttpUrl);
                break;
        }

        return builder.Build();
    }

    public override void Read(byte[] bytes)
    {

    }
}
