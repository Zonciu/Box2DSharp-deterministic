using Box2DSharp.Common;

namespace Box2DSharp.Dynamics.Contacts
{
    public struct ContactVelocityConstraint
    {
        /// <summary>
        /// Size <see cref="Settings.MaxManifoldPoints"/>
        /// </summary>
        public FixedArray2<VelocityConstraintPoint> Points;

        public int ContactIndex;

        public FP Friction;

        public int IndexA;

        public int IndexB;

        public FP InvIa, InvIb;

        public FP InvMassA, InvMassB;

        public Matrix2x2 K;

        public FVector2 Normal;

        public Matrix2x2 NormalMass;

        public int PointCount;

        public FP Restitution;

        public FP Threshold;

        public FP TangentSpeed;
    }
}