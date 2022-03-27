using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Packets.Definitions.Lua;

public class ModInfoItem
{
    public ushort Id { get; set; }
    public uint Hash { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasSize { get; set; }
    public bool HasHashInfo { get; set; }
    public Vector3 Size { get; set; }
    public Vector3 OriginalSize { get; set; }
    public uint ShortBytes { get; set; }
    public string ShortMd5 { get; set; } = string.Empty;
    public string ShortSha256 { get; set; } = string.Empty;
    public uint LongBytes { get; set; }
    public string LongMd5 { get; set; } = string.Empty;
    public string LongSha256 { get; set; } = string.Empty;

    public ModInfoItem()
    {
    }
}
