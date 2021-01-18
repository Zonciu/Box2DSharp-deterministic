using System.Numerics;
using Box2DSharp.Common;

namespace Testbed.Abstractions
{
    public static class MathExtensions
    {
        public static void Set(ref this Vector2 vector2, float x, float y)
        {
            vector2.X = x;
            vector2.Y = y;
        }

        public static FVector2 ToFVector2(in this Vector2 vector2)
        {
            return new FVector2(vector2.X, vector2.Y);
        }

        public static Vector2 ToVector2(in this FVector2 vector2)
        {
            return new Vector2((float)vector2.X, (float)vector2.Y);
        }

        public static FVector3 ToFVector3(in this Vector3 vector3)
        {
            return new FVector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static Vector3 ToVector3(in this FVector3 vector3)
        {
            return new Vector3((float)vector3.X, (float)vector3.Y, (float)vector3.Z);
        }
    }
}