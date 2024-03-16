using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using SlipeServer.Server;
using SlipeServer.Server.Clients;
using System;
using System.IO.Pipes;
using System.Linq;

namespace SlipeServer.Example.Proxy;

public enum RemoteMessageType : byte
{
    Join,
    Quit,
    packet
}

public class ProxyService
{
    private readonly MtaServer server;
    private readonly ILogger logger;
    private readonly NamedPipeClientStream namedPipe;
    private readonly ProxyNetWrapper proxyNetWrapper;

    public ProxyService(MtaServer server, ILogger logger)
    {
        this.server = server;
        this.logger = logger;

        this.namedPipe = new NamedPipeClientStream(".", "mta-server-proxy", PipeDirection.InOut, PipeOptions.Asynchronous);
        this.proxyNetWrapper = new(this);

        server.RegisterNetWrapper(this.proxyNetWrapper);
        server.AddNetWrapper(this.proxyNetWrapper);

        server.ClientConnected += AssignIdOnConnect;

        Init();
    }

    private void AssignIdOnConnect(IClient client)
    {
        client.FetchSerial();
        if (uint.TryParse(client.Extra, out var preferedId))
            client.Player.Id = (ElementId)preferedId;
    }

    public async void Init()
    {
        await this.namedPipe.ConnectAsync();

        byte[] previous = Array.Empty<byte>();
        byte[] buffer = new byte[10240];
        while (true)
        {
            var size = await this.namedPipe.ReadAsync(buffer);

            if (size > 0)
                previous = HandleData(previous.Concat(buffer.Take(size)).ToArray());
        }
    }

    private byte[] HandleData(byte[] data)
    {
        if (data.Length < 5)
            return data;

        var type = (RemoteMessageType)data[0];
        var size = BitConverter.ToUInt32(data, 1);

        if (data.Length < size + 5)
            return data;

        var payload = data.Skip(5).Take((int)size);
        HandleMessage(type, payload.ToArray());

        return HandleData(data.Skip((int)size + 5).ToArray());
    }

    private void HandleMessage(RemoteMessageType type, byte[] data)
    {
        if (type == RemoteMessageType.packet)
        {
            var playerId = BitConverter.ToUInt32(data, 0);
            var packetId = (PacketId)data[4];

            this.proxyNetWrapper.TriggerPacketReceival(playerId, packetId, data.Skip(5).ToArray());
        }
    }

    public void SendMessage(RemoteMessageType type, byte packetId, uint id, byte[] data)
    {
        var payload = new byte[] { (byte)type }
            .Concat(BitConverter.GetBytes((uint)data.Length + 5))
            .Concat(BitConverter.GetBytes(id))
            .Concat(new byte[] { packetId })
            .Concat(data).ToArray();
        this.namedPipe.Write(payload);
    }
}
