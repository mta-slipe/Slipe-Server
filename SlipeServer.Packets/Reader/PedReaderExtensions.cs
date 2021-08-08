using System;
using System.Numerics;

namespace SlipeServer.Packets.Reader
{
    public static class PedReaderExtensions
    {
        public static byte GetBodyPart(this PacketReader reader) => reader.GetByteCapped(3);
    }
}
