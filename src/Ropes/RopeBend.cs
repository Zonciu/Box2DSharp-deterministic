using Box2DSharp.Common;

namespace Box2DSharp.Ropes
{
    public struct RopeBend
    {
        public int i1;

        public int i2;

        public int i3;

        public FP invMass1;

        public FP invMass2;

        public FP invMass3;

        public FP invEffectiveMass;

        public FP lambda;

        public FP L1, L2;

        public FP alpha1;

        public FP alpha2;

        public FP spring;

        public FP damper;
    };
}