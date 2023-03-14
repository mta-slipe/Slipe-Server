using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using SlipeServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SlipeServer.Console.ResourceFeatures;

namespace SlipeServer.Console.AdditionalResources.ResourceWithFeatures;

public class ResourceWithFeaturesResource : Resource, IAddAuthorFeature
#if DEBUG
    , IVerifySourceCodeFeature
#endif
{
    public Dictionary<string, byte[]> AdditionalFiles { get; } = new Dictionary<string, byte[]>()
    {
        ["test.lua"] = ResourceFiles.TestLua,
    };

    public string Author => "Foo Bar";

    public ResourceWithFeaturesResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "ResourceWithFeatures")
    {
        using var md5 = MD5.Create();

        foreach (var (path, content) in this.AdditionalFiles)
            this.Files.Add(ResourceFileFactory.FromBytes(content, path));
    }
}
