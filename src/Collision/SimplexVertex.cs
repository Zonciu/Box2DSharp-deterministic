using Box2DSharp.Common;

namespace Box2DSharp.Collision
{
    public struct SimplexVertex
    {
        public FVector2 Wa; // support point in proxyA

        public FVector2 Wb; // support point in proxyB

        public FVector2 W; // wB - wA

        public FP A; // barycentric coordinate for closest point

        public int IndexA; // wA index

        public int IndexB; // wB index
    }
}