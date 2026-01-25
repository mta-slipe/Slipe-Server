using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Services;

public interface IFireService
{
    void CreateFire(Vector3 position, float size = 1.8F, Player? responsiblePlayer = null);
    void CreateFireFor(IEnumerable<Player> players, Vector3 position, float size = 1.8F, Player? responsiblePlayer = null);
}