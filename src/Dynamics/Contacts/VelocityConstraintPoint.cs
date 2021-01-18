using Box2DSharp.Common;

namespace Box2DSharp.Dynamics.Contacts
{
    public struct VelocityConstraintPoint
    {
        public FP NormalImpulse;

        public FP NormalMass;

        public FVector2 Ra;

        public FVector2 Rb;

        public FP TangentImpulse;

        public FP TangentMass;

        public FP VelocityBias;
    }
}