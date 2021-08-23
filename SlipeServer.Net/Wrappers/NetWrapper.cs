using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Packets;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SlipeServer.Net.Wrappers
{
    public class NetWrapper : IDisposable, INetWrapper
    {
        private const string wrapperDllpath = @"NetModuleWrapper";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void PacketCallback(byte packetId, uint binaryAddress, IntPtr payload, uint payloadSize, bool hasPing, uint ping);


#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport(wrapperDllpath, EntryPoint = "initNetWrapper")]
        private static extern int InitNetWrapper(string path, string idFile, string ip, ushort port, uint playerCount, string serverName, PacketCallback callback);

        [DllImport(wrapperDllpath, EntryPoint = "destroyNetWrapper")]
        private static extern void DestroyNetWrapper(ushort id);

        [DllImport(wrapperDllpath, EntryPoint = "startNetWrapper")]
        private static extern void StartNetWrapper(ushort id);

        [DllImport(wrapperDllpath, EntryPoint = "stopNetWrapper")]
        private static extern void StopNetWrapper(ushort id);

        [DllImport(wrapperDllpath, EntryPoint = "sendPacket")]
        private static extern bool SendPacket(ushort id, uint binaryAddress, byte packetId, IntPtr payload, uint payloadSize, byte priority, byte ordering);

        [DllImport(wrapperDllpath, EntryPoint = "setSocketVersion")]
        private static extern bool SetSocketVersion(ushort id, uint binaryAddress, ushort version);

        [DllImport(wrapperDllpath, EntryPoint = "getClientSerialAndVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern void GetClientSerialAndVersion(ushort id, uint binaryAddress, StringBuilder serial, StringBuilder extra, StringBuilder version);

        [DllImport(wrapperDllpath, EntryPoint = "setChecks")]
        private static extern void SetChecks(ushort id, string szDisableComboACMap, string szDisableACMap, string szEnableSDMap, int iEnableClientChecks, bool bHideAC, string szImgMods);

        [DllImport(wrapperDllpath, EntryPoint = "resendModPackets")]
        private static extern void ResendModPackets(ushort id, uint binaryAddress);
        [DllImport(wrapperDllpath, EntryPoint = "resendPlayerACInfo")]
        private static extern void ResendPlayerACInfo(ushort id, uint binaryAddress);

#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments

        private readonly PacketCallback packetInterceptorDelegate;
        private readonly ushort id;

        public NetWrapper(string directory, string netDllPath, string host, ushort port)
        {
            string idFile = Path.Join(directory, "id");
            Directory.SetCurrentDirectory(directory);

            this.packetInterceptorDelegate = PacketInterceptor;
            int result = InitNetWrapper(Path.Join(directory, netDllPath), idFile, host, port, 1024, "C# server", this.packetInterceptorDelegate);

            if (result < 0)
            {
                throw new Exception($"Unable to start net wrapper. Error code: {result} ({((NetWrapperErrorCode)result)})");
            }
            this.id = (ushort)result;

            Debug.WriteLine($"Net wrapper initialized: {result}");
        }

        public void Dispose()
        {
            DestroyNetWrapper(this.id);
            GC.SuppressFinalize(this);
        }

        public void Start() => StartNetWrapper(this.id);

        public void Stop() => StopNetWrapper(this.id);

        private void SendPacket(uint binaryAddress, byte packetId, byte[] payload, PacketPriority priority, PacketReliability reliability)
        {
            int size = Marshal.SizeOf((byte)0) * payload.Length;
            IntPtr pointer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(payload, 0, pointer, payload.Length);
                SendPacket(this.id, binaryAddress, packetId, pointer, (uint)payload.Length, (byte)priority, (byte)reliability);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        public void SendPacket(uint binaryAddress, Packet packet)
        {
            SendPacket(binaryAddress, (byte)packet.PacketId, packet.Write(), packet.Priority, packet.Reliability);
        }

        public void SendPacket(uint binaryAddress, PacketId packetId, byte[] data, PacketPriority priority = PacketPriority.High, PacketReliability reliability = PacketReliability.ReliableSequenced)
        {
            SendPacket(binaryAddress, (byte)packetId, data, priority, reliability);
        }

        public Tuple<string, string, string> GetClientSerialExtraAndVersion(uint binaryAddress)
        {
            var serial = new StringBuilder(48);
            var extra = new StringBuilder(48);
            var version = new StringBuilder(48);
            GetClientSerialAndVersion(this.id, binaryAddress, serial, extra, version);

            return new Tuple<string, string, string>(serial.ToString(), extra.ToString(), version.ToString());
        }

        public void ResendModPackets(uint binaryAddress)
        {
            ResendModPackets(this.id, binaryAddress);
        }

        public void ResendPlayerACInfo(uint binaryAddress)
        {
            ResendPlayerACInfo(this.id, binaryAddress);
        }

        public void SetVersion(uint binaryAddress, ushort version)
        {
            SetSocketVersion(this.id, binaryAddress, version);
        }

        public void SetAntiCheatConfig(
            IEnumerable<AntiCheat> disabledAntiCheats,
            bool hideAntiCheatFromClient,
            AllowGta3ImgMods allowGta3ImgMods,
            IEnumerable<SpecialDetection> enabledSpecialDetections,
            DataFile disallowedDataFiles
        )
        {
            var defaultDisabledSdArray = (new int[] { 12, 14, 15, 16, 20, 22, 23, 28, 31, 32, 33, 34, 35, 36 })
                .Where(x => !enabledSpecialDetections.Any(y => (int)y == x));

            SetChecks(this.id,
                string.Join('&', defaultDisabledSdArray.Select(x => $"{x}=")),
                string.Join('&', disabledAntiCheats.Select(x => $"{(int)x}=")),
                string.Join('&', enabledSpecialDetections.Select(x => $"{(int)x}=")),

                (int)disallowedDataFiles,
                hideAntiCheatFromClient,
                allowGta3ImgMods.ToString().ToLower());
        }

        private void PacketInterceptor(byte packetId, uint binaryAddress, IntPtr payload, uint payloadSize, bool hasPing, uint ping)
        {
            byte[] data = new byte[payloadSize];
            Marshal.Copy(payload, data, 0, (int)payloadSize);

            PacketId parsedPacketId = (PacketId)packetId;

            this.PacketReceived?.Invoke(this, binaryAddress, parsedPacketId, data, hasPing ? ping : (uint?)null);
        }

        public event Action<INetWrapper, uint, PacketId, byte[], uint?>? PacketReceived;

    }
}
