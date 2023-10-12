
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Features;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Console.ResourceFeatures;

public interface IAddAuthorFeature : IResourceFeature
{
    string Author { get; }
}

internal class AddAuthorFeature : IResourceFeatureApplier<IAddAuthorFeature>
{
    public AddAuthorFeature()
    {

    }

    private byte[] Combine(params byte[][] arrays)
    {
        byte[] rv = new byte[arrays.Sum(a => a.Length)];
        int offset = 0;
        foreach (byte[] array in arrays)
        {
            System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
            offset += array.Length;
        }
        return rv;
    }

    public void Apply(IAddAuthorFeature feature, Resource resource, Dictionary<string, byte[]> files)
    {
        var authorComment = System.Text.UTF8Encoding.UTF8.GetBytes($"-- Author: {feature.Author}\n\n");
        foreach (var item in files)
        {
            files[item.Key] = Combine(authorComment, item.Value);
            var index = resource.Files.IndexOf(resource.Files.Where(x => x.Name == item.Key).First());
            resource.Files[index] = ResourceFileFactory.FromBytes(files[item.Key], item.Key);
        }
    }
}
