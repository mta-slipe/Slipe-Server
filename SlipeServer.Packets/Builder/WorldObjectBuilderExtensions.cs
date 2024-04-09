using SlipeServer.Packets.Definitions.Entities.Structs;
using System;

namespace SlipeServer.Packets.Builder;

public static class WorldObjectBuilderExtensions
{
    public static void Write(this PacketBuilder builder, PositionRotationAnimation positionRotationAnimation, bool resumeMode = true)
    {
        builder.Write(resumeMode);

        if (resumeMode)
        {
            var now = DateTime.UtcNow;
            ulong elapsedTime = (ulong)(now - positionRotationAnimation.StartTime).Ticks / 10000;
            ulong timeRemaining = 0;
            if (positionRotationAnimation.EndTime > now)
            {
                timeRemaining = (ulong)(positionRotationAnimation.EndTime - now).Ticks / 10000;
            }
            builder.WriteCompressed(elapsedTime);
            builder.WriteCompressed(timeRemaining);
        } else
        {
            var duration = (positionRotationAnimation.EndTime - positionRotationAnimation.StartTime).Ticks / 10000;
            builder.WriteCompressed((ulong)duration);
        }

        builder.WriteVector3WithZAsFloat(positionRotationAnimation.SourcePosition);
        builder.Write(positionRotationAnimation.SourceRotation * (MathF.PI / 180));

        builder.WriteVector3WithZAsFloat(positionRotationAnimation.TargetPosition);
        builder.Write(positionRotationAnimation.DeltaRotationMode);
        if (positionRotationAnimation.DeltaRotationMode)
        {
            builder.Write(positionRotationAnimation.DeltaRotation * (MathF.PI / 180));
        } else
        {
            builder.Write(positionRotationAnimation.TargetRotation * (MathF.PI / 180));
        }

        builder.Write(positionRotationAnimation.EasingType);
        builder.Write(positionRotationAnimation.EasingPeriod);
        builder.Write(positionRotationAnimation.EasingAmplitude);
        builder.Write(positionRotationAnimation.EasingOvershoot);
    }
}
