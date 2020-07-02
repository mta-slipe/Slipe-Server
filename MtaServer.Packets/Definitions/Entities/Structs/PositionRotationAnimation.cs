using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Entities.Structs
{
    public struct PositionRotationAnimation
    {
        public Vector3 SourcePosition { get; set; }
        public Vector3 SourceRotation { get; set; }

        public Vector3 TargetPosition { get; set; }
        public Vector3 TargetRotation { get; set; }

        public bool DeltaRotationMode { get; set; }
        public Vector3 DeltaRotation { get; set; }

        public string EasingType { get; set; }
        public double EasingPeriod { get; set; }
        public double EasingAmplitude { get; set; }
        public double EasingOvershoot { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
