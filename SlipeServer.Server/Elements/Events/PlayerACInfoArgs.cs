using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events;

public class PlayerACInfoArgs : EventArgs
{
    public IEnumerable<byte> DetectedACList { get; set; }
    public uint D3D9Size { get; set; }
    public string D3D9MD5 { get; set; }
    public string D3D9SHA256 { get; set; }

    public PlayerACInfoArgs(IEnumerable<byte> detectedACList, uint d3d9Size, string d3d9MD5, string d3d9SHA256)
    {
        this.DetectedACList = detectedACList;
        this.D3D9Size = d3d9Size;
        this.D3D9MD5 = d3d9MD5;
        this.D3D9SHA256 = d3d9SHA256;
    }
}
