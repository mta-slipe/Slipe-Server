using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Services;

/// <summary>
/// Allows for the creation of explosions
/// </summary>
public class ExplosionService(IMtaServer server) : IExplosionService
{
    public void CreateExplosion(Vector3 position, ExplosionType type, Player? responsiblePlayer = null)
    {
        var packet = new ExplosionPacket(responsiblePlayer?.Id, null, position, (byte)type, (ushort)(responsiblePlayer?.Client.Ping ?? 0));
        server.BroadcastPacket(packet);
    }

    public void CreateExplosionFor(
        IEnumerable<Player> players,
        Vector3 position,
        ExplosionType type,
        Player? responsiblePlayer = null
    )
    {
        var packet = new ExplosionPacket(responsiblePlayer?.Id, null, position, (byte)type, (ushort)(responsiblePlayer?.Client.Ping ?? 0));
        packet.SendTo(players);
    }
}
