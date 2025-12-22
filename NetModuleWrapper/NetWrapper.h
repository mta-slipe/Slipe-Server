#include <iostream>
#include "mta/sdk/SharedUtil.h"
#include "mta/core/CDynamicLibrary.h"
#include "mta/sdk/core/CServerInterface.h"
#include "mta/sdk/SharedUtil.h"
#include "mta/sdk/net/Packets.h"
#include "mta/sdk/net/bitstream.h"
#include "mta/sdk/net/CNetServer.h"
#include <bitset>
#include <map>
#include <iomanip>
#include <queue>
#include <mutex>

#ifndef WIN32
#define __stdcall
#endif
typedef void(__stdcall* PacketCallback)(unsigned char, uint64, char[], unsigned long, bool, unsigned int);

struct QueuedPacket {
    NetServerPlayerID socket;
    unsigned char packetId;
    NetBitStreamInterface* bitStream;
    unsigned char priority;
    unsigned char reliability;

    QueuedPacket(NetServerPlayerID socket, unsigned char packetId, NetBitStreamInterface* bitStream, unsigned char priority, unsigned char reliability)
        : socket(socket), packetId(packetId), bitStream(bitStream), priority(priority), reliability(reliability) {

    }
};

class NetWrapper
{
private:
    uint16_t id;
    static uint16_t nextId;
    static std::map<uint16_t, NetWrapper*> netWrappers;
    static std::map<NetServerPlayerID, NetWrapper*> netWrappersPerSocket;

	std::map<uint64, NetServerPlayerID> sockets;
	bool running;
	PacketCallback registeredCallback;
	std::thread runThread;
	std::thread binThread;
    std::queue<QueuedPacket> packetQueue;
    std::mutex mutex;

    CDynamicLibrary networkLibrary;

    void binPulseLoop();
    void runPulseLoop();
    void testMethod();
public:
    CNetServer* network;

    NetWrapper();
    uint16_t getId();

    void destroy();

    bool packetHandler(unsigned char ucPacketID, const NetServerPlayerID& Socket, NetBitStreamInterface* pBitStream, SNetExtraInfo* pNetExtraInfo);
    void sendPacket(uint64 address, unsigned char packetId, unsigned short bitStreamVersion, unsigned char* payload, unsigned long payloadSize, unsigned char priority, unsigned char reliability);
    void setSocketVersion(uint64 address, unsigned short version);
    void resendModPackets(uint64 address);
    void resendACPackets(uint64 address);
    void getClientSerialAndVersion(uint64 address, char* serial, char* extra, char* version);
    std::string getIPAddress(uint64 address);
    int init(const char* netDllFilePath, const char* idFile, const char* ip, unsigned short port, unsigned int playerCount, const char* serverName, PacketCallback callback);
    void start();
    void stop();
    bool isValidSocket(NetServerPlayerID id);

    void SetChecks(const char* szDisableComboACMap, const char* szDisableACMap, const char* szEnableSDMap, int iEnableClientChecks, bool bHideAC, const char* szImgMods);

    static NetWrapper* getNetWrapper(int id);
    static NetWrapper* getNetWrapper(NetServerPlayerID id);
};

