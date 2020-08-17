using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Join
{
    public class JoinedGamePacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_SERVER_JOINEDGAME;
        public override PacketFlags Flags => throw new NotImplementedException();

        public uint ClientId { get; }
        public int PlayerCount { get; }
        public uint RootId { get; }
        public HttpDownloadType HttpDownloadType { get; }
        public ushort HttpPort { get; }
        public string HttpUrl { get; }
        public int HttpConnectionsPerClient { get; }
        public bool IsFakeLagCommandEnabled { get; }
        public int VoiceSampleRate { get; }
        public int VoiceBitRate { get; }
        public int EnableClientChecks { get; }
        public bool IsVoiceEnabled { get; }
        public int VoiceQuality { get; }

        public JoinedGamePacket(
            uint clientId,
            int playerCount,
            uint rootId,
            HttpDownloadType httpDownloadType,
            ushort httpPort,
            string httpUrl,
            int httpConnectionsPerClient,
            int enableClientChecks,
            bool isFakeLagCommandEnabled = false,
            bool isVoiceEnabled = false,
            int voiceSampleRate = 0,
            int voiceQuality = 0,
            int voiceBitRate = 0)
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

            builder.WriteElementId(ClientId);
            builder.Write((byte)PlayerCount);
            builder.WriteElementId(RootId);

            builder.Write(EnableClientChecks);

            builder.Write(IsVoiceEnabled);
            builder.WriteCapped(VoiceSampleRate, 2);
            builder.WriteCapped(VoiceQuality, 4);
            builder.WriteCompressed((uint)VoiceBitRate);

            builder.Write(IsFakeLagCommandEnabled);

            builder.Write(HttpConnectionsPerClient);
            builder.Write((byte)HttpDownloadType);

            switch (HttpDownloadType)
            {
                case HttpDownloadType.HTTP_DOWNLOAD_ENABLED_PORT:
                    builder.Write(HttpPort);
                    break;

                case HttpDownloadType.HTTP_DOWNLOAD_ENABLED_URL:
                    builder.Write(HttpPort);
                    builder.Write(HttpUrl);
                    break;
            }

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
