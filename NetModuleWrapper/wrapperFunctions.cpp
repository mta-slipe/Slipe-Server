#include "NetWrapper.h"


#ifndef WIN32
#define __cdecl
#define BSTR char*
#endif
#if defined _WIN32
#define EXPORT extern "C" __declspec(dllexport)
#else
#define EXPORT extern "C" 
#endif

EXPORT void __cdecl sendPacket(ushort id, unsigned long address, unsigned char packetId, unsigned char* payload, unsigned long payloadSize, unsigned char priority, unsigned char reliability)
{
    NetWrapper::getNetWrapper(id)->sendPacket(address, packetId, payload, payloadSize, priority, reliability);
}

EXPORT void __cdecl setSocketVersion(ushort id, unsigned long address, unsigned short version)
{
    NetWrapper::getNetWrapper(id)->setSocketVersion(address, version);
}

EXPORT BSTR __cdecl getClientSerialAndVersion(ushort id, unsigned long address, uint16_t & serialSize, uint16_t & extraSize, uint16_t & versionSize)
{
    return NetWrapper::getNetWrapper(id)->getClientSerialAndVersion(address, serialSize, extraSize, versionSize);
}

EXPORT void __cdecl resendModPackets(ushort id, unsigned long address)
{
    NetWrapper::getNetWrapper(id)->resendModPackets(address);
}

EXPORT void __cdecl resendPlayerACInfo(ushort id, unsigned long address)
{
    NetWrapper::getNetWrapper(id)->resendACPackets(address);
}

EXPORT int __cdecl initNetWrapper(const char* netDllFilePath, const char* idFile, const char* ip, unsigned short port,
    unsigned int playerCount, const char* serverName, PacketCallback callback)
{
    auto wrapper = new NetWrapper();
    wrapper->init(netDllFilePath, idFile, ip, port, playerCount, serverName, callback);
    return wrapper->getId();
}

EXPORT void __cdecl destroyNetWrapper(ushort id)
{
    auto wrapper = NetWrapper::getNetWrapper(id);
    wrapper->destroy();
    delete wrapper;
}

EXPORT void __cdecl startNetWrapper(ushort id) {
    auto wrapper = NetWrapper::getNetWrapper(id);
    NetWrapper::getNetWrapper(id)->start();
}

EXPORT void __cdecl stopNetWrapper(ushort id) {
    NetWrapper::getNetWrapper(id)->stop();
}

EXPORT void __cdecl setChecks(ushort id, const char* szDisableComboACMap, const char* szDisableACMap, const char* szEnableSDMap,
                                                        int iEnableClientChecks, bool bHideAC, const char* szImgMods)
{
    NetWrapper::getNetWrapper(id)->SetChecks(szDisableComboACMap, szDisableACMap, szEnableSDMap, iEnableClientChecks, bHideAC, szImgMods);
}

//enum ENetworkUsageDirection
//{
//    STATS_INCOMING_TRAFFIC = 0,
//    STATS_OUTGOING_TRAFFIC = 1
//};
//
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
//extern "C" __declspec(dllexport) const void __cdecl DeobfuscateScript(const char* input, uint inputSize, const char* output, uint& outputSize, const char* szScriptName)
//{
//    return false;
//}
//
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
