using System;
using System.IO;

namespace MtaServer.Server.AllSeeingEye
{
    internal static class BinaryWriterExtension
    {
        public static void WriteWithLength(this BinaryWriter bw, string str)
        {
            bw.Write((byte)(str.Length + 1));
            bw.Write(str.AsSpan());
        }

        public static void WriteWithLength(this BinaryWriter bw, int number)
        {
            bw.WriteWithLength(number.ToString());
        }

        public static void WriteWithLength(this BinaryWriter bw, bool boolean)
        {
            bw.WriteWithLength(boolean ? 1 : 0);
        }
    }
}
