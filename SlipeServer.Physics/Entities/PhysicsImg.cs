using RenderWareIo;
using System;

namespace SlipeServer.Physics.Entities;

public class PhysicsImg(string path) : IDisposable
{
    internal ImgFile imgFile = new ImgFile(path);

    public void Dispose()
    {
        this.imgFile.Dispose();
        GC.SuppressFinalize(this);
    }
}
