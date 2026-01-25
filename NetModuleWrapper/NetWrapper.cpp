#include "NetWrapper.h"

// Suppress strcpy/strncpy warnings on Windows (MSVC)
#if defined(_WIN32) && defined(_MSC_VER)
#pragma warning(disable:4996)
#endif

// Debug logging - Set to 0 to disable all debug output
#define DEBUG_LOGGING 0

#if DEBUG_LOGGING
#define Log(x) std::cout << x
#else
#define Log(x)
#endif

bool staticPacketHandler(unsigned char ucPacketID, const NetServerPlayerID& Socket, NetBitStreamInterface* pBitStream, SNetExtraInfo* pNetExtraInfo)
{
    return NetWrapper::getNetWrapper(Socket)->packetHandler(ucPacketID, Socket, pBitStream, pNetExtraInfo);
}

uint16_t NetWrapper::nextId;
std::map<uint16_t, NetWrapper*> NetWrapper::netWrappers;
std::map<NetServerPlayerID, NetWrapper*> NetWrapper::netWrappersPerSocket;

NetWrapper::NetWrapper() {
    this->id = NetWrapper::nextId++;
    NetWrapper::netWrappers[this->id] = this;
}

ushort NetWrapper::getId() {
    return this->id;
}

void NetWrapper::destroy()
{
    NetWrapper::netWrappers.erase(this->id);

    for (auto kvPair : sockets) {
        NetWrapper::netWrappersPerSocket.erase(kvPair.second);
    }
}

bool NetWrapper::packetHandler(unsigned char ucPacketID, const NetServerPlayerID& Socket, NetBitStreamInterface* pBitStream, SNetExtraInfo* pNetExtraInfo)
{
    sockets[Socket.GetIdentifier()] = Socket;

    if (registeredCallback != nullptr && running)
    {
        uint bitCount = pBitStream->GetNumberOfBitsUsed();
        uint byteCount = pBitStream->GetNumberOfBytesUsed();

        char* buffer = new char[byteCount];
        if (byteCount > 0)
            pBitStream->Read(buffer, byteCount);

        bool hasPing = false;
        unsigned int ping = 0;
        if (pNetExtraInfo != nullptr && pNetExtraInfo->m_bHasPing) {
            hasPing = true;
            ping = pNetExtraInfo->m_uiPing;
        }

        registeredCallback(ucPacketID, Socket.GetIdentifier(), buffer, byteCount, hasPing, ping);
        delete[] buffer;
    }

    return true;
}

void NetWrapper::sendPacket(uint64 address, unsigned char packetId, unsigned short bitStreamVersion, unsigned char* payload, unsigned long payloadSize, unsigned char priority, unsigned char reliability)
{
    NetBitStreamInterface* bitStream = network->AllocateNetServerBitStream(bitStreamVersion);
    if (bitStream)
    {
        bitStream->Write(reinterpret_cast<const char*>(payload), payloadSize);
        NetServerPlayerID& socket = sockets[address];
        mutex.lock();
        packetQueue.push(QueuedPacket(socket, packetId, bitStream, priority, reliability));
        mutex.unlock();
    }
}

void NetWrapper::setSocketVersion(uint64 address, unsigned short version)
{
    network->SetClientBitStreamVersion(sockets[address], version);
}

void NetWrapper::resendModPackets(uint64 address)
{
    network->ResendModPackets(sockets[address]);
}

void NetWrapper::resendACPackets(uint64 address)
{
    network->ResendACPackets(sockets[address]);
}

void NetWrapper::getClientSerialAndVersion(uint64 address, char* serial, char* extra, char* version)
{
    NetServerPlayerID& socket = sockets[address];

    // Use static buffers with placement new to avoid both heap allocation and destructor calls
    // The destructors are never called, avoiding the stack corruption issue

    static char serialBuffer[sizeof(SFixedString<32>)];
    static char extraBuffer[sizeof(SFixedString<64>)];
    static char versionBuffer[sizeof(SFixedString<32>)];
    
    // Placement new - constructs in-place without heap allocation
    SFixedString<32>* strSerialTemp = new (serialBuffer) SFixedString<32>();
    SFixedString<64>* strExtraTemp = new (extraBuffer) SFixedString<64>();
    SFixedString<32>* strPlayerVersionTemp = new (versionBuffer) SFixedString<32>();
    
    network->GetClientSerialAndVersion(socket, *strSerialTemp, *strExtraTemp, *strPlayerVersionTemp);
    
    // Copy directly to output parameters
    STRNCPY(serial, (const char*)*strSerialTemp, 48);
    STRNCPY(extra, (const char*)*strExtraTemp, 48);
    STRNCPY(version, (const char*)*strPlayerVersionTemp, 48);
    
    // Don't call destructors - they cause the crash
    // The static buffers will be reused on next call
}

std::string NetWrapper::getIPAddress(uint64 address) {
    auto socket = sockets[address];

    unsigned short port;
    char ipBytes[22];

    network->GetPlayerIP(socket, ipBytes, &port);
    SString str = ipBytes;

    return (std::string)str;
}

void NetWrapper::testMethod() {
   NetBitStreamInterface* bitStream = network->AllocateNetServerBitStream(0);
    Log("Bitstream allocated\n");
    if (bitStream)
    {
        Log("Bitstream truthy\n");

        //bitStream->WriteCompressed((ulong)0);
        //bitStream->WriteNormVector(0.5, 0.5, 0.5);
        //bitStream->Write(128.56f);
        //bitStream->WriteCompressed(0.56f);

        int bitCount = bitStream->GetNumberOfBitsUsed();

        Log("Bitstream number of bits used " << bitCount << "\n");

        bitStream->ResetReadPointer();
        Log("Bitstream read pointer reset\n");

        for (int i = 0; i < bitCount; i++)
            Log((i % 8 == 0 ? ", 0b" : "") << bitStream->ReadBit());

		Log("Bitstream read complete\n");
        network->DeallocateNetServerBitStream(bitStream);
        Log("Bitstream de-allocating\n");
    }
}

int NetWrapper::init(const char* netDllFilePath, const char* idFile, const char* ip, unsigned short port,
    unsigned int playerCount, const char* serverName, PacketCallback callback, unsigned long expectedVersion, unsigned long expectedVersionType)
{
    registeredCallback = callback;

    bool loaded = networkLibrary.Load(netDllFilePath);


    if (!loaded) {
        return -1001;
    }

    typedef unsigned long (*PFNCHECKCOMPATIBILITY)(unsigned long, unsigned long*);
    PFNCHECKCOMPATIBILITY isCompatible = reinterpret_cast<PFNCHECKCOMPATIBILITY>(networkLibrary.GetProcedureAddress("CheckCompatibility"));

    if (!isCompatible)
    {
        return -1002;
    }

    if (!isCompatible(expectedVersion, (unsigned long*)expectedVersionType)) {

        ulong actualVersion = 0;
        isCompatible(1, &actualVersion);

		Log("Version mismatch on " << netDllFilePath << ", expected " << std::hex << expectedVersion << " but got " << actualVersion << std::dec << std::endl);
        return -1003;
    }

    typedef CNetServer* (*InitNetServerInterface)();
    InitNetServerInterface pfnInitNetServerInterface = (InitNetServerInterface)(networkLibrary.GetProcedureAddress("InitNetServerInterface"));
    if (!pfnInitNetServerInterface)
    {
        return -1004;
    }

    Log("Initting\n");
    this->network = pfnInitNetServerInterface();
    Log("Initted\n");


    if (!this->network->InitServerId("server-id.keys")) {
        return -1005;
    }
    Log("Initted ID\n");

    this->network->RegisterPacketHandler(staticPacketHandler);
    Log("Handler registered\n");
    if (!this->network->StartNetwork(ip, port, playerCount, serverName)) {
        return -1006;
    }
    Log("Network started\n");

    try {
        testMethod();
        Log("Test method called\n");
    }
    catch (std::exception e) {
        return -1007;
    }

    binThread = std::thread(&NetWrapper::binPulseLoop, this);
    Log("Thread started\n");
    SetChecks("12=&14=&15=&16=&20=&22=&23=&28=&31=&32=&33=&34=&35=&36=", "", "", 0, 0, "none");
    Log("Checks set\n");
    return 0;
}

void NetWrapper::binPulseLoop() {
    while (!running)
    {
        network->DoPulse();
        network->GetHTTPDownloadManager(EDownloadMode::ASE)->ProcessQueuedFiles();
        std::this_thread::sleep_for(std::chrono::milliseconds(10));
    }
}

void NetWrapper::runPulseLoop() {
    while (running)
    {
        mutex.lock();
        while (!packetQueue.empty()) {
            QueuedPacket& entry = packetQueue.front();

            network->SendPacket(entry.packetId, entry.socket, entry.bitStream, false, static_cast<NetServerPacketPriority>(entry.priority), static_cast<NetServerPacketReliability>(entry.reliability));
            network->DeallocateNetServerBitStream(entry.bitStream);

            packetQueue.pop();
        }
        mutex.unlock();

        network->DoPulse();
        network->GetHTTPDownloadManager(EDownloadMode::ASE)->ProcessQueuedFiles();
        std::this_thread::sleep_for(std::chrono::milliseconds(1));
    }
}

void NetWrapper::start() {
    running = true;
    binThread.join();
    runThread = std::thread(&NetWrapper::runPulseLoop, this);
}

void NetWrapper::stop() {
    running = false;
    runThread.join();
    binThread = std::thread(&NetWrapper::binPulseLoop, this);
}

bool NetWrapper::isValidSocket(NetServerPlayerID id)
{
    return this->network->IsValidSocket(id);
}

void NetWrapper::SetChecks(const char* szDisableComboACMap, const char* szDisableACMap, const char* szEnableSDMap,
    int iEnableClientChecks, bool bHideAC, const char* szImgMods)
{
    this->network->SetChecks(szDisableComboACMap, szDisableACMap, szEnableSDMap, iEnableClientChecks, bHideAC, szImgMods);
}

NetWrapper* NetWrapper::getNetWrapper(int id)
{
    return NetWrapper::netWrappers[id];
}

NetWrapper* NetWrapper::getNetWrapper(NetServerPlayerID id)
{
    if (NetWrapper::netWrappersPerSocket.find(id) == NetWrapper::netWrappersPerSocket.end()) {
        for (auto wrapper : netWrappers) {
            if (wrapper.second->isValidSocket(id)) {
                NetWrapper::netWrappersPerSocket[id] = wrapper.second;
                return wrapper.second;
            }
        }
        return nullptr;
    }

    return NetWrapper::netWrappersPerSocket[id];
}

