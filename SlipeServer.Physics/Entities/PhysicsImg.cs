using RenderWareIo;
using System;

namespace SlipeServer.Physics.Entities;

public class PhysicsImg : IDisposable
{
    internal ImgFile imgFile;

    public PhysicsImg(string path)
    {
        this.imgFile = new ImgFile(path);
    }

    public void Dispose()
    {
        this.imgFile.Dispose();
        GC.SuppressFinalize(this);
    }
}
