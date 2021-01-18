using Box2DSharp.Common;

namespace Box2DSharp.Collision
{
    /// Output for b2Distance.
    public struct DistanceOutput
    {
        /// closest point on shapeA
        public FVector2 PointA;

        /// closest point on shapeB
        public FVector2 PointB;

        public FP Distance;

        /// number of GJK iterations used
        public int Iterations;
    }
}