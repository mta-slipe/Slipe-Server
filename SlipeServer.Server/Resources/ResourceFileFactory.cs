using Force.Crc32;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements.Enums;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SlipeServer.Server.Resources;

public static class ResourceFileFactory
{
    public static ResourceFile FromStream(Stream content, string fileName, ResourceFileType? fileType = null)
    {
        var data = new byte[content.Length];
        content.Read(data, 0, data.Length);

        return FromBytes(data, fileName, fileType);
    }

    public static ResourceFile FromString(string content, string fileName, ResourceFileType? fileType = null)
    {
        return FromBytes(Encoding.Default.GetBytes(content), fileName, fileType);
    }

    public static ResourceFile FromBytes(byte[] content, string fileName, ResourceFileType? fileType = null)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(content);
        var checksum = Crc32Algorithm.Compute(content);

        fileType ??= fileName.EndsWith(".lua") ? ResourceFileType.ClientScript : ResourceFileType.ClientFile;
        return new ResourceFile()
        {
            Name = fileName,
            AproximateSize = content.Length,
            IsAutoDownload = fileType == ResourceFileType.ClientFile ? true : null,
            CheckSum = checksum,
            FileType = (byte)fileType,
            Md5 = hash
        };
    }
}
