using SlipeServer.Server.Enums;

namespace SlipeServer.Server.AllSeeingEye;

public interface IAseQueryService
{
    string? GetRule(string key);
    bool RemoveRule(string key);
    void SetRule(string key, string value);
    string GetVersion(AseVersion version = AseVersion.v1_6);
    byte[] QueryFull(ushort port);
    byte[] QueryLight(ushort port, VersionType version = VersionType.Release);
    byte[] QueryXFireLight();
}
