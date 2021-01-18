using Box2DSharp.Collision.Collider;
using Box2DSharp.Common;

namespace Box2DSharp.Dynamics.Contacts
{
    public struct ContactPositionConstraint
    {
        /// <summary>
        /// Size <see cref="Settings.MaxManifoldPoints"/>
        /// </summary>
        public FixedArray2<FVector2> LocalPoints;

        public int IndexA;

        public int IndexB;

        public FP InvIa, InvIb;

        public FP InvMassA, InvMassB;

        public FVector2 LocalCenterA, LocalCenterB;

        public FVector2 LocalNormal;

        public FVector2 LocalPoint;

        public int PointCount;

        public FP RadiusA, RadiusB;

        public ManifoldType Type;
    }
}