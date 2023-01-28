using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerACInfoArgs : EventArgs
{
    public IEnumerable<byte> DetectedACList { get; }
    public uint D3D9Size { get; }
    public string D3D9MD5 { get; }
    public string D3D9SHA256 { get; }

    public PlayerACInfoArgs(IEnumerable<byte> detectedACList, uint d3d9Size, string d3d9MD5, string d3d9SHA256)
    {
        this.DetectedACList = detectedACList;
        this.D3D9Size = d3d9Size;
        this.D3D9MD5 = d3d9MD5;
        this.D3D9SHA256 = d3d9SHA256;
    }
}
