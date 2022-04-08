using System;
using System.Numerics;

namespace SlipeServer.Server.Extensions;

public static class NumericsExtensions
{
    private static float ToRadians(float x)
    {
        return x * (float)(Math.PI / 180.0);
    }

    private static float ToDegrees(float x)
    {
        return (float)(x * (180.0 / Math.PI));
    }

    public static Quaternion ToQuaternion(this Vector3 rotation)
    {
        return Quaternion.CreateFromYawPitchRoll(ToRadians(rotation.X), ToRadians(rotation.Y), ToRadians(rotation.Z));
    }

    public static Vector3 ToEuler(this Quaternion quaternion)
    {
        float v1 = quaternion.Z;
        float v2 = quaternion.X;
        float v3 = quaternion.Y;
        float v4 = quaternion.W;


        double sinr_cosp = 2.0 * (v4 * v1 + v2 * v3);
        double cosr_cosp = 1.0 - 2.0 * (v1 * v1 + v2 * v2);
        double roll = Math.Atan2(sinr_cosp, cosr_cosp);


        double sinp = 2.0 * (v4 * v2 - v3 * v1);
        double pitch;
        if (Math.Abs(sinp) >= 1)
            pitch = Math.Sign(sinp) > 0 ? Math.PI : -Math.PI;
        else
            pitch = Math.Asin(sinp);


        double siny_cosp = 2.0 * (v4 * v3 + v1 * v2);
        double cosy_cosp = 1.0 - 2.0 * (v2 * v2 + v3 * v3);
        double yaw = Math.Atan2(siny_cosp, cosy_cosp);

        if (yaw < 0)
            yaw += 2 * Math.PI;

        if (pitch < 0)
            pitch += 2 * Math.PI;

        if (roll < 0)
            roll += 2 * Math.PI;

        return new Vector3(ToDegrees((float)yaw), ToDegrees((float)pitch), ToDegrees((float)roll));
    }
}
