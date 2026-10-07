using System;

namespace SlipeServer.Server.PacketHandling.QueueHandlers;

public class QueueHandlerScalingConfig
{
    public int MinWorkerCount { get; set; }
    public int MaxWorkerCount { get; set; }
    public int QueueHighThreshold { get; set; }
    public int QueueLowThreshold { get; set; }
    public int NewWorkerTimeout { get; set; }
    public int WorkerShutdownTimeout { get; set; }

    public QueueHandlerScalingConfig()
    {
        this.MinWorkerCount = 1;
        this.MaxWorkerCount = 10;
        this.QueueHighThreshold = 10;
        this.QueueLowThreshold = 5;
        this.NewWorkerTimeout = 2500;
        this.WorkerShutdownTimeout = 5000;
    }

    public void Validate()
    {
        if (this.MinWorkerCount < 1)
            throw new ArgumentOutOfRangeException(nameof(this.MinWorkerCount), this.MinWorkerCount, "MinWorkerCount must be at least 1.");

        if (this.MaxWorkerCount < this.MinWorkerCount)
            throw new ArgumentOutOfRangeException(nameof(this.MaxWorkerCount), this.MaxWorkerCount, "MaxWorkerCount must not be lower than MinWorkerCount.");

        if (this.QueueLowThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(this.QueueLowThreshold), this.QueueLowThreshold, "QueueLowThreshold must not be negative.");

        if (this.QueueHighThreshold < this.QueueLowThreshold)
            throw new ArgumentOutOfRangeException(nameof(this.QueueHighThreshold), this.QueueHighThreshold, "QueueHighThreshold must not be lower than QueueLowThreshold.");

        if (this.NewWorkerTimeout < 1)
            throw new ArgumentOutOfRangeException(nameof(this.NewWorkerTimeout), this.NewWorkerTimeout, "NewWorkerTimeout must be at least 1 millisecond.");

        if (this.WorkerShutdownTimeout < 0)
            throw new ArgumentOutOfRangeException(nameof(this.WorkerShutdownTimeout), this.WorkerShutdownTimeout, "WorkerShutdownTimeout must not be negative.");
    }

    public static QueueHandlerScalingConfig Aggressive => new()
    {
        MinWorkerCount = 1,
        MaxWorkerCount = 10,
        QueueHighThreshold = 5,
        QueueLowThreshold = 2,
        NewWorkerTimeout = 1000
    };

    public static QueueHandlerScalingConfig Default => new();
}
