using System.Threading;

namespace SlipeServer.Server.Elements.IdGeneration;

public class RangedElementIdGenerator(uint start, uint stop) : IElementIdGenerator
{
    private uint idCounter = start;

    private readonly Lock idLock = new();

    public uint GetId()
    {
        lock (this.idLock)
        {
            this.idCounter++;
            if (this.idCounter > stop)
                this.idCounter = start;

            return this.idCounter;
        }
    }
}
