using BepuUtilities;
using BepuUtilities.Memory;
using System;
using System.Diagnostics;
using System.Threading;

namespace SlipeServer.Physics.Worlds;

public class SimpleThreadDispatcher : IThreadDispatcher, IDisposable
{
    private readonly int threadCount;
    public int ThreadCount => this.threadCount;
    private struct Worker
    {
        public Thread Thread;
        public AutoResetEvent Signal;
    }

    private readonly Worker[] workers;
    private readonly AutoResetEvent finished;

    private readonly BufferPool[] bufferPools;

    public SimpleThreadDispatcher(int threadCount)
    {
        this.threadCount = threadCount;
        this.workers = new Worker[threadCount - 1];
        for (int i = 0; i < this.workers.Length; ++i)
        {
            this.workers[i] = new Worker { Thread = new Thread(WorkerLoop), Signal = new AutoResetEvent(false) };
            this.workers[i].Thread.IsBackground = true;
            this.workers[i].Thread.Start(this.workers[i].Signal);
        }
        this.finished = new AutoResetEvent(false);
        this.bufferPools = new BufferPool[threadCount];
        for (int i = 0; i < this.bufferPools.Length; ++i)
        {
            this.bufferPools[i] = new BufferPool();
        }
    }

    private void DispatchThread(int workerIndex)
    {
        Debug.Assert(this.workerBody != null);
        this.workerBody(workerIndex);

        if (Interlocked.Increment(ref this.completedWorkerCounter) == this.threadCount)
        {
            this.finished.Set();
        }
    }

    private volatile Action<int>? workerBody;
    private int workerIndex;
    private int completedWorkerCounter;

    private void WorkerLoop(object? untypedSignal)
    {
        var signal = untypedSignal as AutoResetEvent;
        while (true)
        {
            signal?.WaitOne();
            if (this.disposed)
                return;
            DispatchThread(Interlocked.Increment(ref this.workerIndex) - 1);
        }
    }

    private void SignalThreads()
    {
        for (int i = 0; i < this.workers.Length; ++i)
        {
            this.workers[i].Signal.Set();
        }
    }

    public void DispatchWorkers(Action<int> workerBody)
    {
        Debug.Assert(this.workerBody == null);
        this.workerIndex = 1; //Just make the inline thread worker 0. While the other threads might start executing first, the user should never rely on the dispatch order.
        this.completedWorkerCounter = 0;
        this.workerBody = workerBody;
        SignalThreads();
        //Calling thread does work. No reason to spin up another worker and block this one!
        DispatchThread(0);
        this.finished.WaitOne();
        this.workerBody = null;
    }

    private volatile bool disposed;
    public void Dispose()
    {
        if (!this.disposed)
        {
            this.disposed = true;
            SignalThreads();
            for (int i = 0; i < this.bufferPools.Length; ++i)
            {
                this.bufferPools[i].Clear();
            }
            foreach (var worker in this.workers)
            {
                worker.Thread.Join();
                worker.Signal.Dispose();
            }
        }
        GC.SuppressFinalize(this);
    }

    public BufferPool GetThreadMemoryPool(int workerIndex)
    {
        return this.bufferPools[workerIndex];
    }
}
