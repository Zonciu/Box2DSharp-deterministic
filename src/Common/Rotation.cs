using System.Runtime.CompilerServices;

namespace Box2DSharp.Common
{
    /// Rotation
    public struct Rotation
    {
        /// Sine and cosine
        public FP Sin;

        public FP Cos;

        public Rotation(FP sin, FP cos)
        {
            Sin = sin;
            Cos = cos;
        }

        /// Initialize from an angle in radians
        public Rotation(FP angle)
        {
            // TODO_ERIN optimize
            Sin = FP.Sin(angle);
            Cos = FP.Cos(angle);
        }

        /// Set using an angle in radians.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(FP angle)
        {
            // TODO_ERIN optimize
            Sin = FP.Sin(angle);
            Cos = FP.Cos(angle);
        }

        /// Set to the identity rotation
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIdentity()
        {
            Sin = 0.0f;
            Cos = 1.0f;
        }

        /// Get the angle in radians
        public FP Angle => FP.Atan2(Sin, Cos);

        /// Get the x-axis
        public FVector2 GetXAxis()
        {
            return new FVector2(Cos, Sin);
        }

        /// Get the u-axis
        public FVector2 GetYAxis()
        {
            return new FVector2(-Sin, Cos);
        }
    }
}