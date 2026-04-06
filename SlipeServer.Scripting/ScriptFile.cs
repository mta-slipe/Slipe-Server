using System;
using System.IO;

namespace SlipeServer.Scripting;

public class ScriptFile(string path, FileMode mode, FileAccess access) : IDisposable
{
    private readonly FileStream stream = new(path, mode, access, FileShare.Read);
    private bool disposed;

    public string Path { get; } = path;

    public FileStream Stream => this.stream;

    public void Dispose()
    {
        if (!this.disposed)
        {
            this.stream.Dispose();
            this.disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
