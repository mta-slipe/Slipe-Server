#include "NetWrapper.h"
#include <cstring>
#include <cstddef>

#if !defined(_WIN32)
#define __cdecl
#define __stdcall
#define BSTR char*
#endif

#if defined(_WIN32)
#define EXPORT extern "C" __declspec(dllexport)
#pragma warning(disable:4996)
#else
#define EXPORT extern "C" __attribute__((visibility("default")))
#endif

// P/Invoke uses ExactSpelling=true for the two new ASE functions.
// MSVC x86 decorates __stdcall symbols, so export plain-name aliases.
#if defined(_MSC_VER) && defined(_M_IX86)
#pragma comment(linker, "/EXPORT:getAsePingStatus=_getAsePingStatus@12")
#pragma comment(linker, "/EXPORT:getAseNetRoute=_getAseNetRoute@12")
#endif

EXPORT void __stdcall sendPacket(
    ushort id,
    uint64 address,
    unsigned char packetId,
    unsigned short bitStreamVersion,
    unsigned char* payload,
    unsigned long payloadSize,
    unsigned char priority,
    unsigned char reliability)
{
    NetWrapper::getNetWrapper(id)->sendPacket(
        address,
        packetId,
        bitStreamVersion,
        payload,
        payloadSize,
        priority,
        reliability);
}

EXPORT void __stdcall setSocketVersion(
    ushort id,
    uint64 address,
    unsigned short version)
{
    NetWrapper::getNetWrapper(id)->setSocketVersion(address, version);
}

EXPORT void __stdcall getClientSerialAndVersion(
    ushort id,
    uint64 address,
    char* serial,
    char* extra,
    char* version)
{
    auto wrapper = NetWrapper::getNetWrapper(id);

    wrapper->getClientSerialAndVersion(
        address,
        serial,
        extra,
        version);
}

EXPORT void __stdcall getPlayerIp(
    ushort id,
    uint64 address,
    char* result)
{
    std::string ip = NetWrapper::getNetWrapper(id)->getIPAddress(address);
    STRNCPY(result, ip.c_str(), 22);
}

EXPORT void __stdcall resendModPackets(
    ushort id,
    uint64 address)
{
    NetWrapper::getNetWrapper(id)->resendModPackets(address);
}

EXPORT void __stdcall resendPlayerACInfo(
    ushort id,
    uint64 address)
{
    NetWrapper::getNetWrapper(id)->resendACPackets(address);
}

EXPORT int __stdcall initNetWrapper(
    const char* netDllFilePath,
    const char* idFile,
    const char* ip,
    unsigned short port,
    unsigned int playerCount,
    const char* serverName,
    PacketCallback callback,
    unsigned long expectedVersion,
    unsigned long expectedVersionType)
{
    NetWrapper* wrapper = new NetWrapper();

    int result = wrapper->init(
        netDllFilePath,
        idFile,
        ip,
        port,
        playerCount,
        serverName,
        callback,
        expectedVersion,
        expectedVersionType);

    if (result != 0)
    {
        delete wrapper;
        return result;
    }

    return static_cast<int>(wrapper->getId());
}

EXPORT void __stdcall destroyNetWrapper(ushort id)
{
    auto wrapper = NetWrapper::getNetWrapper(id);

    wrapper->destroy();
    delete wrapper;
}

EXPORT void __stdcall startNetWrapper(ushort id)
{
    NetWrapper::getNetWrapper(id)->start();
}

EXPORT void __stdcall stopNetWrapper(ushort id)
{
    NetWrapper::getNetWrapper(id)->stop();
}

EXPORT void __stdcall setChecks(
    ushort id,
    const char* szDisableComboACMap,
    const char* szDisableACMap,
    const char* szEnableSDMap,
    int iEnableClientChecks,
    bool bHideAC,
    const char* szImgMods)
{
    NetWrapper::getNetWrapper(id)->SetChecks(
        szDisableComboACMap,
        szDisableACMap,
        szEnableSDMap,
        iEnableClientChecks,
        bHideAC,
        szImgMods);
}

// Returns the payload length without the terminating NUL.
// Negative values indicate an error:
// -1: invalid output buffer
// -2: unavailable network wrapper
// -3: insufficient output capacity
// -4: missing terminator in the native result
static int ReadAseData(
    unsigned short id,
    unsigned char* buffer,
    int capacity,
    bool readPingStatus)
{
    if (buffer == nullptr || capacity <= 0)
        return -1;

    auto wrapper = NetWrapper::getNetWrapper(id);

    if (wrapper == nullptr || wrapper->network == nullptr)
        return -2;

    // Compatibility workaround:
    // the observed net.dll writes an additional NUL at offset 33.
    // Keep that byte inside our storage and check for further writes.
    struct AseResultStorage
    {
        SFixedString<32> value;
        unsigned char extraTerminator;
        unsigned char guard[8];
    };

    static_assert(sizeof(SFixedString<32>) == 33,
        "Unexpected SFixedString layout.");

    static_assert(offsetof(AseResultStorage, extraTerminator) == 33,
        "Unexpected padding before the extra terminator.");

    static_assert(offsetof(AseResultStorage, guard) == 34,
        "Unexpected guard offset.");

    AseResultStorage result{};

    // Explicitly initialize the full character buffer.
    // SFixedString's constructor initializes only its first byte.
    std::memset(&result.value, 0, sizeof(result.value));
    result.extraTerminator = 0xA5;
    std::memset(result.guard, 0xA5, sizeof(result.guard));

    if (readPingStatus)
        wrapper->network->GetPingStatus(&result.value);
    else
        wrapper->network->GetNetRoute(&result.value);

    // Reject writes beyond the one observed extra terminator.
    for (unsigned char byte : result.guard)
    {
        if (byte != 0xA5)
            return -5;
    }

    if (result.extraTerminator != 0xA5 &&
        result.extraTerminator != 0)
    {
        return -5;
    }

    const char* data = result.value;

    int length = 0;

    while (length < 32 && data[length] != '\0')
        ++length;

    if (length == 32 && data[32] != '\0')
        return -4;

    if (length > capacity)
        return -3;

    if (length > 0)
    {
        std::memcpy(
            buffer,
            data,
            static_cast<std::size_t>(length));
    }

    return length;
}

EXPORT int __stdcall getAsePingStatus(
    unsigned short id,
    unsigned char* buffer,
    int capacity)
{
    return ReadAseData(id, buffer, capacity, true);
}

EXPORT int __stdcall getAseNetRoute(
    unsigned short id,
    unsigned char* buffer,
    int capacity)
{
    return ReadAseData(id, buffer, capacity, false);
}