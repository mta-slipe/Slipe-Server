using SlipeServer.Server;
using SlipeServer.Server.Resources;
using System.Security.Cryptography;

namespace SlipeServer.Console.AdditionalResources.Parachute;

public class ParachuteResource : Resource
{
    public Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["utility.lua"] = ResourceFiles.UtilityLua,
        ["parachute_cl.lua"] = ResourceFiles.ParachuteClientLua,
        ["skydiving_cl.lua"] = ResourceFiles.SkydivingClientLua,
        ["openChute.lua"] = ResourceFiles.OpenChuteLua,
        ["client_anim.lua"] = ResourceFiles.ClientAnimationLua,
        ["parachuteopen.mp3"] = ResourceFiles.ParachuteOpenMp3
    };

    public ParachuteResource(IMtaServer server)
        : base(server, server.RootElement, "Parachute")
    {
        using var md5 = MD5.Create();

        foreach (var (path, content) in this.AdditionalFiles)
            this.Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
