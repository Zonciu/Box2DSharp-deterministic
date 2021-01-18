using Box2DSharp.Common;

namespace Box2DSharp.Collision
{
    /// Output results for b2ShapeCast
    public struct ShapeCastOutput
    {
        public FVector2 Point;

        public FVector2 Normal;

        public FP Lambda;

        public int Iterations;
    }
}