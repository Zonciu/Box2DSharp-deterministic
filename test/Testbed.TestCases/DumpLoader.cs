using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using Testbed.Abstractions;

namespace Testbed.TestCases
{
    [TestCase("Bugs", "Dump Loader")]
    public class DumpLoader : TestBase
    {
        private readonly Body m_ball;

        public DumpLoader()
        {
            var chainShape = new ChainShape();
            FVector2[] vertices = { new FVector2(-5, 0), new FVector2(5, 0), new FVector2(5, 5), new FVector2(4, 1), new FVector2(-4, 1), new FVector2(-5, 5) };
            chainShape.CreateLoop(vertices, 6);

            var groundFixtureDef = new FixtureDef();
            groundFixtureDef.Density = 0;
            groundFixtureDef.Shape = chainShape;

            var groundBodyDef = new BodyDef();
            groundBodyDef.BodyType = BodyType.StaticBody;

            var groundBody = World.CreateBody(groundBodyDef);
            var groundBodyFixture = groundBody.CreateFixture(groundFixtureDef);

            var ballShape = new CircleShape();
            ballShape.Radius = 1;

            var ballFixtureDef = new FixtureDef();
            ballFixtureDef.Restitution = 0.75f;
            ballFixtureDef.Density = 1;
            ballFixtureDef.Shape = ballShape;

            var ballBodyDef = new BodyDef();
            ballBodyDef.BodyType = BodyType.DynamicBody;
            ballBodyDef.Position = new FVector2(0, 10);

            // ballBodyDef.angularDamping = 0.2f;

            m_ball = World.CreateBody(ballBodyDef);
            var ballFixture = m_ball.CreateFixture(ballFixtureDef);
            m_ball.ApplyForceToCenter(new FVector2(-1000, -400), true);
        }

        /// <inheritdoc />
        protected override void PreStep()
        {
            var v = m_ball.LinearVelocity;
            var omega = m_ball.AngularVelocity;

            var massData = m_ball.GetMassData();

            var ke = 0.5f * massData.Mass * FVector2.Dot(v, v) + 0.5f * massData.RotationInertia * omega * omega;

            DrawString($"kinetic energy = {ke}");
        }
    }
}