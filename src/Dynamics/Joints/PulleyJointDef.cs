using System.Diagnostics;
using Box2DSharp.Common;

namespace Box2DSharp.Dynamics.Joints
{
    /// Pulley joint definition. This requires two ground anchors,
    /// two dynamic body anchor points, and a pulley ratio.
    public class PulleyJointDef : JointDef
    {
        /// The first ground anchor in world coordinates. This point never moves.
        public FVector2 GroundAnchorA;

        /// The second ground anchor in world coordinates. This point never moves.
        public FVector2 GroundAnchorB;

        /// The a reference length for the segment attached to bodyA.
        public FP LengthA;

        /// The a reference length for the segment attached to bodyB.
        public FP LengthB;

        /// The local anchor point relative to bodyA's origin.
        public FVector2 LocalAnchorA;

        /// The local anchor point relative to bodyB's origin.
        public FVector2 LocalAnchorB;

        /// The pulley ratio, used to simulate a block-and-tackle.
        public FP Ratio;

        public PulleyJointDef()
        {
            JointType = JointType.PulleyJoint;

            GroundAnchorA.Set(-1.0f, 1.0f);

            GroundAnchorB.Set(1.0f, 1.0f);

            LocalAnchorA.Set(-1.0f, 0.0f);

            LocalAnchorB.Set(1.0f, 0.0f);

            LengthA = 0.0f;

            LengthB = 0.0f;

            Ratio = 1.0f;

            CollideConnected = true;
        }

        /// Initialize the bodies, anchors, lengths, max lengths, and ratio using the world anchors.
        public void Initialize(
            Body bA,
            Body bB,
            in FVector2 groundA,
            in FVector2 groundB,
            in FVector2 anchorA,
            in FVector2 anchorB,
            FP r)
        {
            BodyA = bA;
            BodyB = bB;
            GroundAnchorA = groundA;
            GroundAnchorB = groundB;
            LocalAnchorA = BodyA.GetLocalPoint(anchorA);
            LocalAnchorB = BodyB.GetLocalPoint(anchorB);
            var dA = anchorA - groundA;
            LengthA = dA.Length();
            var dB = anchorB - groundB;
            LengthB = dB.Length();
            Ratio = r;
            Debug.Assert(Ratio > Settings.Epsilon);
        }
    }
}