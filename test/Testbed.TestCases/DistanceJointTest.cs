using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Joints;
using Testbed.Abstractions;

namespace Testbed.TestCases
{
    [TestCase("Joints", "Distance Joint")]
    public class DistanceJointTest : TestBase
    {
        public DistanceJoint m_joint;

        public FP m_length;

        public FP m_minLength;

        public FP m_maxLength;

        public FP m_hertz;

        public FP m_dampingRatio;

        public DistanceJointTest()
        {
            Body ground = null;
            {
                BodyDef bd = new BodyDef();
                ground = World.CreateBody(bd);

                EdgeShape shape = new EdgeShape();
                shape.SetTwoSided(new FVector2(-40.0f, 0.0f), new FVector2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }
            {
                BodyDef bd = new BodyDef();
                bd.BodyType = BodyType.DynamicBody;
                bd.AngularDamping = 0.1f;

                bd.Position.Set(0.0f, 5.0f);
                Body body = World.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(0.5f, 0.5f);
                body.CreateFixture(shape, 5.0f);

                m_hertz = 1.0f;
                m_dampingRatio = 0.7f;

                DistanceJointDef jd = new DistanceJointDef();
                jd.Initialize(ground, body, new FVector2(0.0f, 15.0f), bd.Position);
                jd.CollideConnected = true;
                m_length = jd.Length;
                m_minLength = m_length;
                m_maxLength = m_length;
                JointUtils.LinearStiffness(out jd.Stiffness, out jd.Damping, m_hertz, m_dampingRatio, jd.BodyA, jd.BodyB);
                m_joint = (DistanceJoint)World.CreateJoint(jd);
            }
        }

        /// <inheritdoc />
        protected override void OnGUI()
        {
            DrawString("This demonstrates a soft distance joint.");
            DrawString("Press: (b) to delete a Body, (j) to delete a joint");
        }
    }
}