﻿using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Joints;

namespace NETCoreTest
{
    public class Tumbler
    {
        private const int Count = 200;

        private RevoluteJoint _joint;

        private int _count;

        public World World;

        public Tumbler()
        {
            World = new World();
            Body ground;
            {
                var bd = new BodyDef();
                ground = World.CreateBody(bd);
            }

            {
                var bd = new BodyDef
                {
                    BodyType = BodyType.DynamicBody,
                    AllowSleep = false,
                    Position = new FVector2(0.0f, 10.0f)
                };
                var body = World.CreateBody(bd);

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
                _joint = (RevoluteJoint)World.CreateJoint(jd);
            }

            _count = 0;
        }

        private FP step = FP.One / 60;

        public void Step()
        {
            if (_count < Count)
            {
                var bd = new BodyDef
                {
                    BodyType = BodyType.DynamicBody,
                    Position = new FVector2(0.0f, 10.0f)
                };
                var body = World.CreateBody(bd);

                var shape = new PolygonShape();
                shape.SetAsBox(0.125f, 0.125f);
                body.CreateFixture(shape, 1.0f);

                ++_count;
            }

            World.Step(step, 8, 3);
        }
    }
}