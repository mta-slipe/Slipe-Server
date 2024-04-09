using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public class PositionRotationAnimation
{
    public Vector3 SourcePosition { get; set; }
    public Vector3 SourceRotation { get; set; }

    public Vector3 TargetPosition { get; set; }
    public Vector3 TargetRotation { get; set; }

    public bool DeltaRotationMode { get; set; }
    public Vector3 DeltaRotation { get; set; }

    public Vector3 DeltaPosition => this.TargetPosition - this.SourcePosition;

    public string EasingType { get; set; } = "linear";
    public double EasingPeriod { get; set; }
    public double EasingAmplitude { get; set; }
    public double EasingOvershoot { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Progress => (DateTime.UtcNow - this.StartTime).TotalMicroseconds / (this.EndTime - this.StartTime).TotalMicroseconds;
}
