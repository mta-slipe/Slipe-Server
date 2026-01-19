#include "NetWrapper.h"
#include <iostream>


#ifndef WIN32
#define __cdecl
#define __stdcall
#define BSTR char*
#endif
#if defined _WIN32
#define EXPORT extern "C" __declspec(dllexport)
#pragma warning(disable:4996)
#else
#define EXPORT extern "C" __attribute__ ((visibility ("default")))
#endif

EXPORT void __stdcall sendPacket(ushort id, uint64 address, unsigned char packetId, unsigned short bitStreamVersion, unsigned char* payload, unsigned long payloadSize, unsigned char priority, unsigned char reliability)
{
    NetWrapper::getNetWrapper(id)->sendPacket(address, packetId, bitStreamVersion, payload, payloadSize, priority, reliability);
}

EXPORT void __stdcall setSocketVersion(ushort id, uint64 address, unsigned short version)
{
    NetWrapper::getNetWrapper(id)->setSocketVersion(address, version);
}

EXPORT void __stdcall getClientSerialAndVersion(ushort id, uint64 address, char* serial, char* extra, char* version)
{
    auto wrapper = NetWrapper::getNetWrapper(id);
    wrapper->getClientSerialAndVersion(address, serial, extra, version);
}

EXPORT void __stdcall getPlayerIp(ushort id, uint64 address, char* result)
{
    std::string ip = NetWrapper::getNetWrapper(id)->getIPAddress(address);
    STRNCPY(result, ip.c_str(), 22);
}

EXPORT void __stdcall resendModPackets(ushort id, uint64 address)
{
    NetWrapper::getNetWrapper(id)->resendModPackets(address);
}

EXPORT void __stdcall resendPlayerACInfo(ushort id, uint64 address)
{
    NetWrapper::getNetWrapper(id)->resendACPackets(address);
}

EXPORT int __stdcall initNetWrapper(const char* netDllFilePath, const char* idFile, const char* ip, unsigned short port,
    unsigned int playerCount, const char* serverName, PacketCallback callback)
{
    NetWrapper* wrapper = new NetWrapper();
    wrapper->init(netDllFilePath, idFile, ip, port, playerCount, serverName, callback);
    uint16_t id = wrapper->getId();

    return (int)id;
}

EXPORT void __stdcall destroyNetWrapper(ushort id)
{
    auto wrapper = NetWrapper::getNetWrapper(id);
    wrapper->destroy();
    delete wrapper;
}

EXPORT void __stdcall startNetWrapper(ushort id) {
    auto wrapper = NetWrapper::getNetWrapper(id);
    NetWrapper::getNetWrapper(id)->start();
}

EXPORT void __stdcall stopNetWrapper(ushort id) {
    NetWrapper::getNetWrapper(id)->stop();
}

EXPORT void __stdcall setChecks(ushort id, const char* szDisableComboACMap, const char* szDisableACMap, const char* szEnableSDMap,
                                                        int iEnableClientChecks, bool bHideAC, const char* szImgMods)
{
    NetWrapper::getNetWrapper(id)->SetChecks(szDisableComboACMap, szDisableACMap, szEnableSDMap, iEnableClientChecks, bHideAC, szImgMods);
}
