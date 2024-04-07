#include "NetWrapper.h"

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
    sockets[Socket.GetBinaryAddress()] = Socket;

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

        registeredCallback(ucPacketID, Socket.GetBinaryAddress(), buffer, byteCount, hasPing, ping);
        delete[] buffer;
    }

    return true;
}

void NetWrapper::sendPacket(unsigned long address, unsigned char packetId, unsigned short bitStreamVersion, unsigned char* payload, unsigned long payloadSize, unsigned char priority, unsigned char reliability)
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

void NetWrapper::setSocketVersion(unsigned long address, unsigned short version)
{
    network->SetClientBitStreamVersion(sockets[address], version);
}

void NetWrapper::resendModPackets(unsigned long address)
{
    network->ResendModPackets(sockets[address]);
}

void NetWrapper::resendACPackets(unsigned long address)
{
    network->ResendACPackets(sockets[address]);
}

SerialExtraAndVersion NetWrapper::getClientSerialAndVersion(unsigned long address)
{
    auto socket = sockets[address];

    SFixedString<32> strSerialTemp;
    SFixedString<64> strExtraTemp;
    SFixedString<32> strPlayerVersionTemp;
    network->GetClientSerialAndVersion(socket, strSerialTemp, strExtraTemp, strPlayerVersionTemp);

    std::string serial = (std::string)SStringX(strSerialTemp);
    std::string extra = (std::string)SStringX(strExtraTemp);
    std::string version = (std::string)SStringX(strPlayerVersionTemp);

    SerialExtraAndVersion result = SerialExtraAndVersion(serial, extra, version);
    return result;
}

std::string NetWrapper::getIPAddress(unsigned long address) {
    auto socket = sockets[address];

    unsigned short port;
    char ipBytes[22];

    network->GetPlayerIP(socket, ipBytes, &port);
    SString str = ipBytes;

    return (std::string)str;
}

void NetWrapper::testMethod() {
    NetBitStreamInterface* bitStream = network->AllocateNetServerBitStream(0);
    if (bitStream)
    {
        //bitStream->WriteCompressed((ulong)0);
        //bitStream->WriteNormVector(0.5, 0.5, 0.5);
        //bitStream->Write(128.56f);
        //bitStream->WriteCompressed(0.56f);

        int bitCount = bitStream->GetNumberOfBitsUsed();

        bitStream->ResetReadPointer();

        for (int i = 0; i < bitCount; i++)
            std::cout << (i % 8 == 0 ? ", 0b" : "") << bitStream->ReadBit();

        network->DeallocateNetServerBitStream(bitStream);
    }
}

int NetWrapper::init(const char* netDllFilePath, const char* idFile, const char* ip, unsigned short port,
    unsigned int playerCount, const char* serverName, PacketCallback callback)
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
    if (!isCompatible(0x0AB, (unsigned long*)0x09)) {

        ulong actualVersion = 0;
        if (isCompatible)
            isCompatible(1, &actualVersion);

        return -1003;
    }

    typedef CNetServer* (*InitNetServerInterface)();
    InitNetServerInterface pfnInitNetServerInterface = (InitNetServerInterface)(networkLibrary.GetProcedureAddress("InitNetServerInterface"));
    if (!pfnInitNetServerInterface)
    {
        return -1004;
    }
    ;

    this->network = pfnInitNetServerInterface();


    if (!this->network->InitServerId("server-id.keys")) {
        return -1005;
    }
    this->network->RegisterPacketHandler(staticPacketHandler);
    if (!this->network->StartNetwork(ip, port, playerCount, serverName)) {
        return -1006;
    }

    testMethod();

    binThread = std::thread(&NetWrapper::binPulseLoop, this);
    SetChecks("12=&14=&15=&16=&20=&22=&23=&28=&31=&32=&33=&34=&35=&36=", "", "", 0, 0, "none");
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

