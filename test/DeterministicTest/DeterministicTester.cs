using System;
using System.Security.Cryptography;
using System.Text;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Joints;

namespace DeterministicTest
{
    public class DeterministicTester
    {
        public (string Hash, string Data) TestTumbler(int totalSteps, int blockCount)
        {
            var world = new World();
            int _count;

            Body ground;
            {
                var bd = new BodyDef();
                ground = world.CreateBody(bd);
            }

            {
                var bd = new BodyDef
                {
                    BodyType = BodyType.DynamicBody,
                    AllowSleep = false,
                    Position = new FVector2(0.0f, 10.0f)
                };
                var body = world.CreateBody(bd);

                var shape = new PolygonShape();
                shape.SetAsBox(0.5f, 10.0f, new FVector2(10.0f, 0.0f), 0.0f);
                body.CreateFixture(shape, 5.0f);
                shape.SetAsBox(0.5f, 10.0f, new FVector2(-10.0f, 0.0f), 0.0f);
                body.CreateFixture(shape, 5.0f);
                shape.SetAsBox(10.0f, 0.5f, new FVector2(0.0f, 10.0f), 0.0f);
                body.CreateFixture(shape, 5.0f);
                shape.SetAsBox(10.0f, 0.5f, new FVector2(0.0f, -10.0f), 0.0f);
                body.CreateFixture(shape, 5.0f);

                var jd = new RevoluteJointDef
                {
                    BodyA = ground,
                    BodyB = body,
                    LocalAnchorA = new FVector2(0.0f, 10.0f),
                    LocalAnchorB = new FVector2(0.0f, 0.0f),
                    ReferenceAngle = 0.0f,
                    MotorSpeed = 0.05f * Settings.Pi,
                    MaxMotorTorque = 1e8f,
                    EnableMotor = true
                };
                world.CreateJoint(jd);
            }

            _count = 0;
            FP invDt = FP.One / 60;
            var data = new StringBuilder();
            for (int i = 0; i < totalSteps; i++)
            {
                while (_count < blockCount)
                {
                    var bd = new BodyDef
                    {
                        BodyType = BodyType.DynamicBody,
                        Position = new FVector2(0.0f, 10.0f)
                    };
                    var body = world.CreateBody(bd);

                    var shape = new PolygonShape();
                    shape.SetAsBox(0.125f, 0.125f);
                    body.CreateFixture(shape, 1.0f);

                    ++_count;
                }

                world.Step(invDt, 8, 3);
                data.Append($"Step {i} begin\n");
                foreach (var body in world.BodyList)
                {
                    data.Append(body.GetTransform().ToString());
                    data.Append("\n");
                }

                data.Append($"Step {i} end\n");
            }

            var dataString = data.ToString();
            var hash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(dataString))).Replace("-", "");
            return (hash, dataString);
        }

        public (string Hash, string Data) TestTheoJansen(int totalSteps)
        {
            Body _chassis;

            RevoluteJoint _motorJoint;

            bool _motorOn;

            FP _motorSpeed = FP.Zero;

            FVector2 _offset = FVector2.Zero;

            Body _wheel;
            World World = new World(new FVector2(0, -10));
            _offset.Set(0.0f, 8.0f);
            _motorSpeed = 2.0f;
            _motorOn = true;
            var pivot = new FVector2(0.0f, 0.8f);

            // Ground
            {
                var bd = new BodyDef();
                var ground = World.CreateBody(bd);

                var shape = new EdgeShape();
                shape.SetTwoSided(new FVector2(-50.0f, 0.0f), new FVector2(50.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);

                shape.SetTwoSided(new FVector2(-50.0f, 0.0f), new FVector2(-50.0f, 10.0f));
                ground.CreateFixture(shape, 0.0f);

                shape.SetTwoSided(new FVector2(50.0f, 0.0f), new FVector2(50.0f, 10.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            // Balls
            for (var i = 0; i < 40; ++i)
            {
                var shape = new CircleShape();
                shape.Radius = 0.25f;

                var bd = new BodyDef();
                bd.BodyType = BodyType.DynamicBody;
                bd.Position.Set(-40.0f + 2.0f * i, 0.5f);

                var body = World.CreateBody(bd);
                body.CreateFixture(shape, 1.0f);
            }

            // Chassis
            {
                var shape = new PolygonShape();
                shape.SetAsBox(2.5f, 1.0f);

                var sd = new FixtureDef();
                sd.Density = 1.0f;
                sd.Shape = shape;
                sd.Filter.GroupIndex = -1;
                var bd = new BodyDef();
                bd.BodyType = BodyType.DynamicBody;
                bd.Position = pivot + _offset;
                _chassis = World.CreateBody(bd);
                _chassis.CreateFixture(sd);
            }

            {
                var shape = new CircleShape();
                shape.Radius = 1.6f;
                var sd = new FixtureDef();
                sd.Density = 1.0f;
                sd.Shape = shape;
                sd.Filter.GroupIndex = -1;
                var bd = new BodyDef();
                bd.BodyType = BodyType.DynamicBody;
                bd.Position = pivot + _offset;
                _wheel = World.CreateBody(bd);
                _wheel.CreateFixture(sd);
            }

            {
                var jd = new RevoluteJointDef();
                jd.Initialize(_wheel, _chassis, pivot + _offset);
                jd.CollideConnected = false;
                jd.MotorSpeed = _motorSpeed;
                jd.MaxMotorTorque = 400.0f;
                jd.EnableMotor = _motorOn;
                _motorJoint = (RevoluteJoint)World.CreateJoint(jd);
            }

            FVector2 wheelAnchor;

            wheelAnchor = pivot + new FVector2(0.0f, -0.8f);

            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.GetPosition(), 120 * FP.Pi / 180);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            _wheel.SetTransform(_wheel.GetPosition(), -120 * FP.Pi / 180);
            CreateLeg(-1.0f, wheelAnchor);
            CreateLeg(1.0f, wheelAnchor);

            FP invDt = FP.One / 60;
            var data = new StringBuilder();
            for (int i = 0; i < totalSteps; i++)
            {
                World.Step(invDt, 8, 3);
                data.Append($"Step {i} begin\n");
                foreach (var body in World.BodyList)
                {
                    data.Append(body.GetTransform().ToString());
                    data.Append("\n");
                }

                data.Append($"Step {i} end\n");
            }

            var dataString = data.ToString();
            var hash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(dataString))).Replace("-", "");
            return (hash, dataString);

            void CreateLeg(FP s, FVector2 anchor)
            {
                var p1 = new FVector2(5.4f * s, -6.1f);
                var p2 = new FVector2(7.2f * s, -1.2f);
                var p3 = new FVector2(4.3f * s, -1.9f);
                var p4 = new FVector2(3.1f * s, 0.8f);
                var p5 = new FVector2(6.0f * s, 1.5f);
                var p6 = new FVector2(2.5f * s, 3.7f);

                var fd1 = new FixtureDef {Filter = {GroupIndex = -1}, Density = 1.0f};
                var fd2 = new FixtureDef {Filter = {GroupIndex = -1}, Density = 1.0f};

                var poly1 = new PolygonShape();
                var poly2 = new PolygonShape();

                if (s > 0.0f)
                {
                    var vertices = new FVector2[3];

                    vertices[0] = p1;
                    vertices[1] = p2;
                    vertices[2] = p3;
                    poly1.Set(vertices);

                    vertices[0] = FVector2.Zero;
                    vertices[1] = p5 - p4;
                    vertices[2] = p6 - p4;
                    poly2.Set(vertices);
                }
                else
                {
                    var vertices = new FVector2[3];

                    vertices[0] = p1;
                    vertices[1] = p3;
                    vertices[2] = p2;
                    poly1.Set(vertices);

                    vertices[0] = FVector2.Zero;
                    vertices[1] = p6 - p4;
                    vertices[2] = p5 - p4;
                    poly2.Set(vertices);
                }

                fd1.Shape = poly1;
                fd2.Shape = poly2;

                var bd1 = new BodyDef();
                var bd2 = new BodyDef();
                bd1.BodyType = BodyType.DynamicBody;
                bd2.BodyType = BodyType.DynamicBody;
                bd1.Position = _offset;
                bd2.Position = p4 + _offset;

                bd1.AngularDamping = 10.0f;
                bd2.AngularDamping = 10.0f;

                var body1 = World.CreateBody(bd1);
                var body2 = World.CreateBody(bd2);

                body1.CreateFixture(fd1);
                body2.CreateFixture(fd2);

                {
                    var jd = new DistanceJointDef();

                    // Using a soft distance constraint can reduce some jitter.
                    // It also makes the structure seem a bit more fluid by
                    // acting like a suspension system.
                    var dampingRatio = 0.5f;
                    var frequencyHz = 10.0f;

                    jd.Initialize(body1, body2, p2 + _offset, p5 + _offset);
                    JointUtils.LinearStiffness(out jd.Stiffness, out jd.Damping, frequencyHz, dampingRatio, jd.BodyA, jd.BodyB);
                    World.CreateJoint(jd);

                    jd.Initialize(body1, body2, p3 + _offset, p4 + _offset);
                    JointUtils.LinearStiffness(out jd.Stiffness, out jd.Damping, frequencyHz, dampingRatio, jd.BodyA, jd.BodyB);
                    World.CreateJoint(jd);

                    jd.Initialize(body1, _wheel, p3 + _offset, anchor + _offset);
                    JointUtils.LinearStiffness(out jd.Stiffness, out jd.Damping, frequencyHz, dampingRatio, jd.BodyA, jd.BodyB);
                    World.CreateJoint(jd);

                    jd.Initialize(body2, _wheel, p6 + _offset, anchor + _offset);
                    JointUtils.LinearStiffness(out jd.Stiffness, out jd.Damping, frequencyHz, dampingRatio, jd.BodyA, jd.BodyB);
                    World.CreateJoint(jd);
                }

                {
                    var jd = new RevoluteJointDef();
                    jd.Initialize(body2, _chassis, p4 + _offset);
                    World.CreateJoint(jd);
                }
            }
        }
    }
}