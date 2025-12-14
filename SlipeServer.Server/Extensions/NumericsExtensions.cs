using System;
using System.Numerics;

namespace SlipeServer.Server.Extensions;

public static class NumericsExtensions
{
    /// <summary>
    /// Converts a value in degrees to the equivalent in radians
    /// </summary>
    private static float ToRadians(float x)
    {
        return x * (float)(Math.PI / 180.0);
    }

    /// <summary>
    /// Converts a value in radians to its equivalent in degrees
    /// </summary>
    private static float ToDegrees(float x)
    {
        return (float)(x * (180.0 / Math.PI));
    }

    /// <summary>
    /// Return the equivalent representation of a rotation, in euler angles, in quaternions
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static Quaternion ToQuaternion(this Vector3 rotation)
    {
        return Quaternion.CreateFromYawPitchRoll(ToRadians(rotation.X), ToRadians(rotation.Y), ToRadians(rotation.Z));
    }

    /// <summary>
    /// Returns the equivalent representation of a quaternion in euler angles
    /// </summary>
    /// <param name="quaternion"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the equivalent representation of a quaternion in euler angles
    /// </summary>
    /// <param name="quaternion"></param>
    /// <returns></returns>
    public static Vector3 ToEulerFromBeppu(this Quaternion quaternion)
    {
        double qx = quaternion.X;
        double qy = quaternion.Y;
        double qz = quaternion.Z;
        double qw = quaternion.W;

        double norm = Math.Sqrt(qx * qx + qy * qy + qz * qz + qw * qw);
        qx /= norm; qy /= norm; qz /= norm; qw /= norm;

        // ZXY extraction
        double rotX, rotZ, rotY;

        // rotX (pitch around X)
        rotX = Math.Asin(Math.Clamp(2 * (qw * qx - qy * qz), -1.0, 1.0));

        // rotY (roll around Y)
        rotZ = Math.Atan2(2 * (qw * qy + qx * qz), 1 - 2 * (qx * qx + qy * qy));

        // rotZ (yaw around Z)
        rotY = Math.Atan2(2 * (qw * qz + qx * qy), 1 - 2 * (qx * qx + qz * qz));

        // Convert to degrees
        rotX *= 180.0 / Math.PI;
        rotZ *= 180.0 / Math.PI;
        rotY *= 180.0 / Math.PI;

        return new Vector3((float)rotX, (float)rotY, (float)-rotZ);
    }
}
