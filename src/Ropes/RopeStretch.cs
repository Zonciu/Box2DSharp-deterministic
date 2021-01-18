using Box2DSharp.Common;

namespace Box2DSharp.Ropes
{
    public struct RopeStretch
    {
        public int I1;

        public int I2;

        public FP InvMass1;

        public FP InvMass2;

        public FP L;

        public FP Lambda;

        public FP Spring;

        public FP Damper;
    };
}