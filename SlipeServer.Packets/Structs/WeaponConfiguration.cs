namespace SlipeServer.Packets.Structs
{
    public struct WeaponConfiguration
    {
        public int WeaponType { get; set; }
        public float TargetRange { get; set; }
        public float WeaponRange { get; set; }

        public int Flags { get; set; }

        public short Ammo { get; set; }
        public short Damage { get; set; }

        public float Accuracy { get; set; }
        public float MoveSpeed { get; set; }

        public float AnimationLoopStart { get; set; }
        public float AnimationLoopStop { get; set; }
        public float AnimationLoopBulletFire { get; set; }

        public float Animation2LoopStart { get; set; }
        public float Animation2LoopStop { get; set; }
        public float Animation2LoopBulletFire { get; set; }

        public float AnimationBreakoutTime { get; set; }
    }
}
