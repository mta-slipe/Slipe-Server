using MtaServer.Net;
using MtaServer.Packets;
using MtaServer.Packets.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MtaServer.Net
{
    public class NetWrapper
    {
        const string wrapperDllpath = @"NetModuleWrapper.dll";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void PacketCallback(byte packetId, uint binaryAddress, IntPtr payload, uint payloadSize);


        [DllImport(wrapperDllpath, EntryPoint = "initNetWrapper")]
        private static extern int InitNetWrapper(string path, string idFile, string ip, ushort port, uint playerCount, string serverName, PacketCallback callback);
        
        [DllImport(wrapperDllpath, EntryPoint = "startNetWrapper")]
        private static extern void StartNetWrapper();

        [DllImport(wrapperDllpath, EntryPoint = "stopNetWrapper")]
        private static extern void StopNetWrapper();

        [DllImport(wrapperDllpath, EntryPoint = "sendPacket")]
        private static extern bool SendPacket(uint binaryAddress, byte packetId, IntPtr payload, uint payloadSize);

        [DllImport(wrapperDllpath, EntryPoint = "setSocketVersion")]
        private static extern bool SetSocketVersion(uint binaryAddress, ushort version);

        [DllImport(wrapperDllpath, EntryPoint = "getClientSerialAndVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string GetClientSerialAndVersion(uint binaryAddress, out ushort serialSize, out ushort extraSize, out ushort versionSize);

        private readonly PacketCallback packetInterceptorDelegate;
        
        public NetWrapper(string directory, string netDllPath, string host, ushort port)
        {
            string idFile = Path.Join(directory, "id");
            Directory.SetCurrentDirectory(directory);

            packetInterceptorDelegate = PacketInterceptor;
            int result = InitNetWrapper(netDllPath, idFile, host, port, 1024, "C# server", packetInterceptorDelegate);

            if (result != 0)
            {
                throw new Exception($"Unable to start net wrapper. Error code: {result} ({((NetWrapperErrorCode)result)})");
            }

            Debug.WriteLine($"Net wrapper initialized: {result}");
        }

        public void Start()
        {
            StartNetWrapper();
        }

        public void Stop()
        {
            StopNetWrapper();
        }

        void SendPacket(uint binaryAddress, byte packetId, byte[] payload)
        {
            int size = Marshal.SizeOf((byte)0) * payload.Length;
            IntPtr pointer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(payload, 0, pointer, payload.Length);
                SendPacket(binaryAddress, packetId, pointer, (uint)payload.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        public Tuple<string, string, string> GetClientSerialExtraAndVersion(uint binaryAddress)
        {
            string result = GetClientSerialAndVersion(binaryAddress, out ushort serialSize, out ushort extraSize, out _).ToString();

            string serial = result.Substring(0, serialSize);
            string extra = result.Substring(serialSize, extraSize);
            string version = result.Substring(serialSize + extraSize);
            return new Tuple<string, string, string>(serial, extra, version);
        }

        public void SendPacket(uint binaryAddress, Packet packet)
        {
            SendPacket(binaryAddress, (byte)packet.PacketId, packet.Write());
        }

        public void SendPacket(uint binaryAddress, PacketId packetId, byte[] data)
        {
            SendPacket(binaryAddress, (byte)packetId, data);
        }

        public void SetVersion(uint binaryAddress, ushort version)
        {
            SetSocketVersion(binaryAddress, version);
        }

        void PacketInterceptor(byte packetId, uint binaryAddress, IntPtr payload, uint payloadSize)
        {
            byte[] data = new byte[payloadSize];
            Marshal.Copy(payload, data, 0, (int)payloadSize);

            PacketId parsedPacketId = (PacketId)packetId;
                
            this.OnPacketReceived?.Invoke(this, binaryAddress, parsedPacketId, data);
        }

        public event Action<NetWrapper, uint, PacketId, byte[]>? OnPacketReceived;

    }
}
