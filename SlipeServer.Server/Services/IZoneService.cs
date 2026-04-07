using System.Numerics;

namespace SlipeServer.Server.Services;

public interface IZoneService
{
    string GetZoneName(Vector3 position, bool citiesOnly = false);
}
