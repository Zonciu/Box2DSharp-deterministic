using Box2DSharp.Common;

namespace Box2DSharp.Collision.Shapes
{
    public struct MassData
    {
        /// The mass of the shape, usually in kilograms.
        public FP Mass;

        /// The position of the shape's centroid relative to the shape's origin.
        public FVector2 Center;

        /// The rotational inertia of the shape about the local origin.
        public FP RotationInertia;
    }
}