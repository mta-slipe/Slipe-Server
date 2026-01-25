using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Services;

public interface IExplosionService
{
    void CreateExplosion(Vector3 position, ExplosionType type, Player? responsiblePlayer = null);
    void CreateExplosionFor(IEnumerable<Player> players, Vector3 position, ExplosionType type, Player? responsiblePlayer = null);
}