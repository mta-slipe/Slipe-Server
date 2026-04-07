namespace SlipeServer.Server.Services;

public interface IZoneService
{
    string GetZoneName(float x, float y, float z, bool citiesOnly = false);
}
