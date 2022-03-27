using SlipeServer.Packets.Definitions.Fire;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Services;

public class FireService
{
    private readonly MtaServer server;

    public FireService(MtaServer server)
    {
        this.server = server;
    }

    public void CreateFire(Vector3 position, float size = 1.8f, Player? responsiblePlayer = null)
    {
        var packet = new FirePacket(position, size, responsiblePlayer?.Id, (ushort?)responsiblePlayer?.Client.Ping);
        this.server.BroadcastPacket(packet);
    }

    public void CreateFireFor(
        IEnumerable<Player> players,
        Vector3 position,
        float size = 1.8f,
        Player? responsiblePlayer = null
    )
    {
        var packet = new FirePacket(position, size, responsiblePlayer?.Id, (ushort?)responsiblePlayer?.Client.Ping);
        packet.SendTo(players);
    }
}
