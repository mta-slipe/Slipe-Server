using System;
using System.Numerics;
using static System.MathF;

namespace SlipeServer.Server.Extensions
{
    public static class Vector3Extensions
    {
        public static bool IsNearAnotherPoint3D(this Vector3 value, Vector3 the3DPoint, float maxDistance)
        {
            float distanceX = the3DPoint.X - value.X;
            float distanceY = the3DPoint.Y - value.Y;
            float distanceZ = the3DPoint.Z - value.Z;


            return ((distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ) <=
                    maxDistance * maxDistance);
        }

        public static bool IsNearAnotherPoint2D(this Vector3 value, Vector3 secondPos, float maxDistance)
        {
            float distanceX = secondPos.X - value.X;
            float distanceY = secondPos.Y - value.Y;


            return ((distanceX * distanceX + distanceY * distanceY) <= maxDistance * maxDistance);
        }

        public static float GetDistanceToTarget3D(this Vector3 value, Vector3 secondPos)
        {
            float distanceX = secondPos.X - value.X;
            float distanceY = secondPos.Y - value.Y;
            float distanceZ = secondPos.Z - value.Z;

            return Sqrt(distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ);
        }
    }
}
