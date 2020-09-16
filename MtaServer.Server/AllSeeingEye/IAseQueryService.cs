namespace MtaServer.Server.AllSeeingEye
{
    public interface IAseQueryService
    {
        string? GetRule(string key);
        bool RemoveRule(string key);
        void SetRule(string key, string value);
        string GetVersion();
        byte[] QueryFull();
        byte[] QueryLight();
        byte[] QueryXFireLight();
    }
}