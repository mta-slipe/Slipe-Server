#include "StdInc.h"
#include <iostream>
#include "../Server/core/CDynamicLibrary.h"
#include "../Server/sdk/core/CServerInterface.h";
#include <net\Packets.h>
//#include "../Server/mods/deathmatch/logic/packets/CPacket.h"
#include <bitset>
#include <map>
#include <SharedUtil.h>

enum ENetworkUsageDirection
{
    STATS_INCOMING_TRAFFIC = 0,
    STATS_OUTGOING_TRAFFIC = 1
};

CNetServer* network;

std::map<ulong, NetServerPlayerID> sockets;

typedef void(__stdcall* PacketCallback)(unsigned char, unsigned long, char[], unsigned long);
PacketCallback registeredCallback;

bool packetHandler(unsigned char ucPacketID, const NetServerPlayerID& Socket, NetBitStreamInterface* pBitStream, SNetExtraInfo* pNetExtraInfo)
{
    sockets[Socket.GetBinaryAddress()] = Socket;
    if (registeredCallback != nullptr)
    {
        uint bitCount = pBitStream->GetNumberOfBitsUsed();
        uint byteCount = pBitStream->GetNumberOfBytesUsed();

        char buffer[4096];
        pBitStream->Read(buffer, byteCount);

        registeredCallback(ucPacketID, Socket.GetBinaryAddress(), buffer, byteCount);
    }

    return false;
}

extern "C" __declspec(dllexport) void __cdecl sendPacket(unsigned long address, unsigned char packetId, unsigned char* payload, unsigned long payloadSize)
{
    NetBitStreamInterface* pBitStream = network->AllocateNetServerBitStream(0);
    if (pBitStream)
    {
        for (int i = 0; i < payloadSize; i++)
        {
            pBitStream->Write((char)payload[i]);
        }

        auto socket = sockets[address];
        network->SendPacket(packetId, socket, pBitStream, false, PACKET_PRIORITY_HIGH, PACKET_RELIABILITY_RELIABLE_ORDERED);
        network->DeallocateNetServerBitStream(pBitStream);
    }
}

extern "C" __declspec(dllexport) const
    char* __cdecl getClientSerialAndVersion(unsigned long address, uint16_t& serialSize, uint16_t& extraSize, uint16_t& versionSize)
{
    auto socket = sockets[address];

    SFixedString<32> strSerialTemp;
    SFixedString<64> strExtraTemp;
    SFixedString<32> strPlayerVersionTemp;
    network->GetClientSerialAndVersion(socket, strSerialTemp, strExtraTemp, strPlayerVersionTemp);

    std::string serial = (std::string)SStringX(strSerialTemp);
    std::string extra = (std::string)SStringX(strExtraTemp);
    std::string version = (std::string)SStringX(strPlayerVersionTemp);

    serialSize = serial.length();
    extraSize = extra.length();
    versionSize = version.length();

    std::string result = serial + extra + version;
    return result.c_str();
}

extern "C" __declspec(dllexport) bool __cdecl initNetWrapper(const char* netDllFilePath, const char* idFile, const char* ip, unsigned short port,
                                                             unsigned int playerCount, const char* serverName, PacketCallback callback)
{
    registeredCallback = callback;
    CDynamicLibrary networkLibrary;

    networkLibrary.Load(netDllFilePath);

    typedef unsigned long (*PFNCHECKCOMPATIBILITY)(unsigned long, unsigned long*);
    PFNCHECKCOMPATIBILITY isCompatible = reinterpret_cast<PFNCHECKCOMPATIBILITY>(networkLibrary.GetProcedureAddress("CheckCompatibility"));

    if (!isCompatible)
    {
        throw std::exception("Can't detect compatibility");
    }

    typedef CNetServer* (*InitNetServerInterface)();
    InitNetServerInterface pfnInitNetServerInterface = (InitNetServerInterface)(networkLibrary.GetProcedureAddress("InitNetServerInterface"));
    if (!pfnInitNetServerInterface)
    {
        throw std::exception("Unable to get net server interface");
    }

    network = pfnInitNetServerInterface();

    std::cout << "Attempting to start '" << serverName << "' on " << ip << ":" << port << " (" << playerCount << ")\n";

    network->InitServerId("");
    network->RegisterPacketHandler(packetHandler);
    network->StartNetwork(ip, port, playerCount, serverName);

    SNetOptions netOptions;
    netOptions.netFilter.bValid = true;
    netOptions.netFilter.bAutoFilter = true;

    netOptions.netOptimize.bValid = true;
    netOptions.netOptimize.iUpdateCycleDatagramsLimit = 10;
    netOptions.netOptimize.iUpdateCycleMessagesLimit = 10;

    network->SetNetOptions(netOptions);

    while (true)
    {
        network->DoPulse();
        std::this_thread::sleep_for(std::chrono::milliseconds(10));
    }
    return true;
}

// extern "C" __declspec(dllexport) void StopNetwork()
//{
//    network->StopNetwork();
//}
//
// extern "C" __declspec(dllexport) void __cdecl DoPulse()
//{
//    network->DoPulse();
//}
//
// extern "C" __declspec(dllexport) void __cdecl RegisterPacketHandler(PPACKETHANDLER pfnPacketHandler)
//{
//    network->RegisterPacketHandler(pfnPacketHandler);
//}
//
// extern "C" __declspec(dllexport) bool __cdecl GetNetworkStatistics(NetStatistics* pDest, const NetServerPlayerID& PlayerID)
//{
//}
//
// extern "C" __declspec(dllexport) const SPacketStat* __cdecl GetPacketStats()
//{
//}
//
// extern "C" __declspec(dllexport) bool __cdecl GetBandwidthStatistics(SBandwidthStatistics* pDest)
//{
//}
//
// extern "C" __declspec(dllexport) bool __cdecl GetNetPerformanceStatistics(SNetPerformanceStatistics* pDest, bool bResetCounters)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl GetPingStatus(SFixedString<32>* pstrStatus)
//{
//}
//
// extern "C" __declspec(dllexport) bool __cdecl GetSyncThreadStatistics(SSyncThreadStatistics* pDest, bool bResetCounters)
//{
//}
//
// extern "C" __declspec(dllexport) NetBitStreamInterface
//    __cdecl* AllocateNetServerBitStream(unsigned short usBitStreamVersion, const void* pData = nullptr, uint uiDataSize = 0, bool bCopyData = false)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl DeallocateNetServerBitStream(NetBitStreamInterface* bitStream)
//{
//}
//
// extern "C" __declspec(dllexport) bool __cdecl SendPacket(unsigned char ucPacketID, const NetServerPlayerID& playerID, NetBitStreamInterface* bitStream,
//                                                         bool bBroadcast, NetServerPacketPriority packetPriority, NetServerPacketReliability
//                                                         packetReliability, ePacketOrdering packetOrdering = PACKET_ORDERING_DEFAULT)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl GetPlayerIP(const NetServerPlayerID& playerID, char strIP[22], unsigned short* usPort)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl Kick(const NetServerPlayerID& PlayerID)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl SetPassword(const char* szPassword)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl SetMaximumIncomingConnections(unsigned short numberAllowed)
//{
//}
//
// extern "C" __declspec(dllexport) CNetHTTPDownloadManagerInterface __cdecl* GetHTTPDownloadManager(EDownloadModeType iMode)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl SetClientBitStreamVersion(const NetServerPlayerID& PlayerID, unsigned short usBitStreamVersion)
//{
//}
// extern "C" __declspec(dllexport) void __cdecl ClearClientBitStreamVersion(const NetServerPlayerID& PlayerID)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl SetChecks(const char* szDisableComboACMap, const char* szDisableACMap, const char* szEnableSDMap,
//                                                        int iEnableClientChecks, bool bHideAC, const char* szImgMods)
//{
//}
//
// extern "C" __declspec(dllexport) unsigned int __cdecl GetPendingPacketCount()
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl GetNetRoute(SFixedString<32>* pstrRoute)
//{
//}
//
// extern "C" __declspec(dllexport) bool __cdecl InitServerId(const char* szPath)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl ResendModPackets(const NetServerPlayerID& playerID)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl ResendACPackets(const NetServerPlayerID& playerID)
//{
//}
//
//
// extern "C" __declspec(dllexport) void __cdecl SetNetOptions(const SNetOptions& options)
//{
//}
//
// extern "C" __declspec(dllexport) void __cdecl GenerateRandomData(void* pOutData, uint uiLength)
//{
//}
//
// extern "C" __declspec(dllexport) bool __cdecl EncryptDumpfile(const char* szClearPathFilename, const char* szEncryptedPathFilename)
//{
//    return false;
//}
//
// extern "C" __declspec(dllexport) bool __cdecl ValidateHttpCacheFileName(const char* szFilename)
//{
//    return false;
//}
//
// extern "C" __declspec(dllexport) bool __cdecl GetScriptInfo(const char* cpInBuffer, uint uiInSize, SScriptInfo* pOutInfo)
//{
//    return false;
//}
//
// extern "C" __declspec(dllexport) bool __cdecl DeobfuscateScript(const char* cpInBuffer, uint uiInSize, const char** pcpOutBuffer, uint* puiOutSize,
//                                                                const char* szScriptName)
//{
//    return false;
//}
//
// extern "C" __declspec(dllexport) bool __cdecl GetPlayerPacketUsageStats(uchar* packetIdList, uint uiNumPacketIds, SPlayerPacketUsage* pOutStats,
//                                                                        uint uiTopCount)
//{
//    return false;
//}
//
// extern "C" __declspec(dllexport) const char* __cdecl GetLogOutput()
//{
//    return NULL;
//}
//
// extern "C" __declspec(dllexport) bool __cdecl IsValidSocket(const NetServerPlayerID& playerID)
//{
//    assert(0);
//    return false;
//}
