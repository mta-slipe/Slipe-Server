namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public struct QueueHandlerScalingConfig
    {
        public int MinWorkerCount { get; set; }
        public int MaxWorkerCount { get; set; }
        public int QueueHighThreshold { get; set; }
        public int QueueLowThreshold { get; set; }
        public int NewWorkerTimeout { get; set; }

        public static QueueHandlerScalingConfig Aggressive => new ()
        {
            MinWorkerCount = 1,
            MaxWorkerCount = 10,
            QueueHighThreshold = 5,
            QueueLowThreshold = 2,
            NewWorkerTimeout = 1000
        };

        public static QueueHandlerScalingConfig Default => new ()
        {
            MinWorkerCount = 1,
            MaxWorkerCount = 10,
            QueueHighThreshold = 10,
            QueueLowThreshold = 5,
            NewWorkerTimeout = 2500
        };
    }
}
