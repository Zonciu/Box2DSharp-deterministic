using Box2DSharp.Common;

namespace Box2DSharp.Dynamics.Joints
{
    /// Friction joint definition.
    public class FrictionJointDef : JointDef
    {
        /// The local anchor point relative to bodyA's origin.
        public FVector2 LocalAnchorA;

        /// The local anchor point relative to bodyB's origin.
        public FVector2 LocalAnchorB;

        /// The maximum friction force in N.
        public FP MaxForce;

        /// The maximum friction torque in N-m.
        public FP MaxTorque;

        public FrictionJointDef()
        {
            JointType = JointType.FrictionJoint;
            LocalAnchorA.SetZero();
            LocalAnchorB.SetZero();
            MaxForce = 0.0f;
            MaxTorque = 0.0f;
        }

        // Point-to-point constraint
        // Cdot = v2 - v1
        //      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
        // J = [-I -r1_skew I r2_skew ]
        // Identity used:
        // w k % (rx i + ry j) = w * (-ry i + rx j)

        // Angle constraint
        // Cdot = w2 - w1
        // J = [0 0 -1 0 0 1]
        // K = invI1 + invI2
        /// Initialize the bodies, anchors, axis, and reference angle using the world
        /// anchor and world axis.
        public void Initialize(Body bA, Body bB, in FVector2 anchor)
        {
            BodyA = bA;
            BodyB = bB;
            LocalAnchorA = BodyA.GetLocalPoint(anchor);
            LocalAnchorB = BodyB.GetLocalPoint(anchor);
        }
    }
}