namespace SlipeServer.Server.Elements.IdGeneration;

public class RangedElementIdGenerator : IElementIdGenerator
{
    private readonly uint start;
    private readonly uint stop;
    private uint idCounter;

    private readonly object idLock = new();

    public RangedElementIdGenerator(uint start, uint stop)
    {
        this.idCounter = start;
        this.start = start;
        this.stop = stop;
    }

    public uint GetId()
    {
        lock (this.idLock)
        {
            this.idCounter++;
            if (this.idCounter > this.stop)
                this.idCounter = this.start;

            return this.idCounter;
        }
    }
}
