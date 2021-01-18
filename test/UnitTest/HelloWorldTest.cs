using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    public class HelloWorldTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HelloWorldTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact(DisplayName = "Hello World")]
        public void HelloWorld()
        {
            // Define the gravity vector.
            FVector2 gravity = new FVector2(0.0f, -10.0f);

            // Construct a world object, which will hold and simulate the rigid bodies.
            World world = new World(gravity);

            // Define the ground body.
            BodyDef groundBodyDef = new BodyDef();
            groundBodyDef.Position.Set(0.0f, -10.0f);

            // Call the body factory which allocates memory for the ground body
            // from a pool and creates the ground box shape (also from a pool).
            // The body is also added to the world.
            var groundBody = world.CreateBody(groundBodyDef);

            // Define the ground box shape.
            PolygonShape groundBox = new PolygonShape();

            // The extents are the half-widths of the box.
            groundBox.SetAsBox(50.0f, 10.0f);

            // Add the ground fixture to the ground body.
            groundBody.CreateFixture(groundBox, 0.0f);

            // Define the dynamic body. We set its position and call the body factory.
            BodyDef bodyDef = new BodyDef();
            bodyDef.BodyType = BodyType.DynamicBody;
            bodyDef.Position.Set(0.0f, 4.0f);
            var body = world.CreateBody(bodyDef);

            // Define another box shape for our dynamic body.
            PolygonShape dynamicBox = new PolygonShape();
            dynamicBox.SetAsBox(1.0f, 1.0f);

            // Define the dynamic body fixture.
            FixtureDef fixtureDef = new FixtureDef();
            fixtureDef.Shape = dynamicBox;

            // Set the box density to be non-zero, so it will be dynamic.
            fixtureDef.Density = 1.0f;

            // Override the default friction.
            fixtureDef.Friction = 0.3f;

            // Add the shape to the body.
            body.CreateFixture(fixtureDef);

            // Prepare for simulation. Typically we use a time step of 1/60 of a
            // second (60Hz) and 10 iterations. This provides a high quality simulation
            // in most game scenarios.
            FP timeStep = 1.0f / 60.0f;
            var velocityIterations = 6;
            var positionIterations = 2;

            FVector2 position = body.GetPosition();
            var angle = body.GetAngle();

            // This is our little game loop.
            for (var i = 0; i < 60; ++i)
            {
                // Instruct the world to perform a single step of simulation.
                // It is generally best to keep the time step and iterations fixed.
                world.Step(timeStep, velocityIterations, positionIterations);

                // Now print the position and angle of the body.
                position = body.GetPosition();
                angle = body.GetAngle();

                _testOutputHelper.WriteLine($"{position.X:F2},{position.Y:F2},{angle:F2}");
            }

            // When the world destructor is called, all bodies and joints are freed. This can
            // create orphaned pointers, so be careful about your world management.

            FP.Abs(position.X).ShouldBeLessThan<FP>(0.01f);
            FP.Abs(position.Y - 1.01f).ShouldBeLessThan<FP>(0.01f);
            FP.Abs(angle).ShouldBeLessThan<FP>(0.01f);
        }
    }
}