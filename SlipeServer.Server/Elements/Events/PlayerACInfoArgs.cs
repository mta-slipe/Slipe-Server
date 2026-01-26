using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerACInfoArgs(IEnumerable<byte> detectedACList, uint d3d9Size, string d3d9MD5, string d3d9SHA256) : EventArgs
{
    public IEnumerable<byte> DetectedACList { get; } = detectedACList;
    public uint D3D9Size { get; } = d3d9Size;
    public string D3D9MD5 { get; } = d3d9MD5;
    public string D3D9SHA256 { get; } = d3d9SHA256;
}
