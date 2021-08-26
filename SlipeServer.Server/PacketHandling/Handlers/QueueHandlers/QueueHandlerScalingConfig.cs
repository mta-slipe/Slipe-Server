namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class QueueHandlerScalingConfig
    {
        public int MinWorkerCount { get; set; }
        public int MaxWorkerCount { get; set; }
        public int QueueHighThreshold { get; set; }
        public int QueueLowThreshold { get; set; }
        public int NewWorkerTimeout { get; set; }

        public QueueHandlerScalingConfig()
        {
            this.MinWorkerCount = 1;
            this.MaxWorkerCount = 10;
            this.QueueHighThreshold = 10;
            this.QueueLowThreshold = 5;
            this.NewWorkerTimeout = 2500;
        }

        public static QueueHandlerScalingConfig Aggressive => new ()
        {
            MinWorkerCount = 1,
            MaxWorkerCount = 10,
            QueueHighThreshold = 5,
            QueueLowThreshold = 2,
            NewWorkerTimeout = 1000
        };

        public static QueueHandlerScalingConfig Default => new();
    }
}
