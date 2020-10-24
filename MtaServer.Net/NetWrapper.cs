using System.Collections.Generic;
using MtaServer.Packets;
using MtaServer.Packets.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

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

        [DllImport(wrapperDllpath, EntryPoint = "setChecks")]
        private static extern void SetChecks(string szDisableComboACMap, string szDisableACMap, string szEnableSDMap, int iEnableClientChecks, bool bHideAC, string szImgMods);

        private static dynamic[] gtaDataFiles = new[] {
            new { fileName = "data/animgrp.dat", bitNumber = 1 },
            new { fileName = "data/ar_stats.dat", bitNumber = 3 },
            new { fileName = "data/carmods.dat", bitNumber = 0 },
            new { fileName = "data/clothes.dat", bitNumber = 5 },
            new { fileName = "data/default.dat", bitNumber = 7 },
            new { fileName = "data/default.ide", bitNumber = 9 },
            new { fileName = "data/gta.dat", bitNumber = 11 },
            new { fileName = "data/maps", bitNumber = 25 },
            new { fileName = "data/object.dat", bitNumber = 6 },
            new { fileName = "data/peds.ide", bitNumber = 13 },
            new { fileName = "data/pedstats.dat", bitNumber = 15 },
            new { fileName = "data/txdcut.ide", bitNumber = 17 },
            new { fileName = "data/vehicles.ide", bitNumber = 14 },
            new { fileName = "data/weapon.dat", bitNumber = 20 },
            new { fileName = "data/melee.dat", bitNumber = 4 },
            new { fileName = "data/water.dat", bitNumber = 16 },
            new { fileName = "data/water1.dat", bitNumber = 18 },
            new { fileName = "data/handling.cfg", bitNumber = 2 },
            new { fileName = "models/coll/weapons.col", bitNumber = 19 },
            new { fileName = "data/plants.dat", bitNumber = 21 },
            new { fileName = "data/furnitur.dat", bitNumber = 23 },
            new { fileName = "data/procobj.dat", bitNumber = 24 },
            new { fileName = "data/surface.dat", bitNumber = 8 },
            new { fileName = "data/surfinfo.dat", bitNumber = 12 },
            new { fileName = "anim/ped.ifp", bitNumber = 22 },
            new { fileName = "data/timecyc.dat", bitNumber = 26 },
        };

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

        public void SetACConfig(string disabledAC, bool hideAC, string allowGta3ImgMods, string enabledSD, dynamic[] fileChecks)
        {
            if (!new string[] { "none", "peds" }.Contains(allowGta3ImgMods))
            {
                throw new ArgumentException("allowGta3ImgMods has incorrect value! Allowed values: none, peds");
            }

            var defaultDisabledSDArray = new string[] { "12", "14", "15", "16", "20", "22", "23", "28", "31", "32", "33", "34", "35", "36" };
            var szDisabledComboAC = new SzArgMap();
            var szDisabledAC = new SzArgMap();
            var szEnabledSD = new SzArgMap();
            var enableClientChecks = -1;
            foreach (var i in defaultDisabledSDArray)
            {
                szDisabledComboAC.Set(i, "");
            }

            var parsedDisabledAC = disabledAC.Split(",");
            var parsedEnabledSD = enabledSD.Split(",");
            foreach (var f in parsedDisabledAC)
            {
                if (int.TryParse(f, out _))
                {
                    szDisabledAC.Set(f, "");
                    szDisabledComboAC.Set(f, "");
                }
            }
            foreach (var sd in parsedEnabledSD)
            {
                if (int.TryParse(sd, out _))
                {
                    szEnabledSD.Set(sd, "");
                    szDisabledComboAC.Remove(sd);
                }
            }

            foreach (var fc in fileChecks)
            {
                var gtaFile = gtaDataFiles.Where(x => x.fileName == fc.FileName).FirstOrDefault();
                if (gtaFile != null)
                {
                    if (fc.Verify)
                    {
                        enableClientChecks |= 1 << gtaFile.bitNumber;
                    }
                    else
                    {
                        enableClientChecks &= ~(1 << gtaFile.bitNumber);
                    }
                }
            }

            SetChecks(szDisabledComboAC.ToString(),
                      szDisabledAC.ToString(),
                      szEnabledSD.ToString(),
                      enableClientChecks,
                      hideAC,
                      allowGta3ImgMods);
        }

        // This class is used only for AC configuring, to keep compatibility with internal implementation of NetModuleWrapper.dll
        internal class SzArgMap
        {
            private IDictionary<string, string> internalMap = new Dictionary<string, string>();
            public void Set(string key, string value)
            {
                internalMap[key] = value;
            }

            public void Remove(string key)
            {
                internalMap.Remove(key);
            }

            public override string ToString()
            {
                var resultingString = "";
                foreach (var kv in internalMap)
                {
                    if (resultingString.Length == 0) resultingString += $"{kv.Key}={kv.Value}";
                    resultingString += $"&{kv.Key}={kv.Value}";
                }
                return resultingString;
            }
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
