using Box2DSharp.Common;

namespace Box2DSharp.Collision.Collider
{
    /// Used for computing contact manifolds.
    public struct ClipVertex
    {
        public FVector2 Vector;

        public ContactId Id;
    }
}