using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Shouldly;
using Xunit;

namespace UnitTest
{
    public class CollisionTest
    {
        [Fact(DisplayName = "polygon mass data")]
        public void PolygonMassData()
        {
            var center = new FVector2(100.0f, -50.0f);
            FP hx = 0.5f, hy = 1.5f;
            FP angle1 = 0.25f;

            // Data from issue #422. Not used because the data exceeds accuracy limits.
            //const b2Vec2 center(-15000.0f, -15000.0f);
            //const float hx = 0.72f, hy = 0.72f;
            //const float angle1 = 0.0f;

            PolygonShape polygon1 = new PolygonShape();
            polygon1.SetAsBox(hx, hy, center, angle1);

            var absTol = 2.0f * Settings.Epsilon;
            var relTol = 2.0f * Settings.Epsilon;

            FP.Abs(polygon1.Centroid.X - center.X).ShouldBeLessThan(absTol + relTol * FP.Abs(center.X));
            FP.Abs(polygon1.Centroid.Y - center.Y).ShouldBeLessThan(absTol + relTol * FP.Abs(center.Y));

            var vertices = new FVector2[4];
            vertices[0].Set(center.X - hx, center.Y - hy);
            vertices[1].Set(center.X + hx, center.Y - hy);
            vertices[2].Set(center.X - hx, center.Y + hy);
            vertices[3].Set(center.X + hx, center.Y + hy);

            PolygonShape polygon2 = new PolygonShape();
            polygon2.Set(vertices, 4);

            FP.Abs(polygon2.Centroid.X - center.X).ShouldBeLessThan(absTol + relTol * FP.Abs(center.X));
            FP.Abs(polygon2.Centroid.Y - center.Y).ShouldBeLessThan(absTol + relTol * FP.Abs(center.Y));

            var mass = 4.0f * hx * hy;
            var inertia = (mass / 3.0f) * (hx * hx + hy * hy) + mass * FVector2.Dot(center, center);

            polygon1.ComputeMass(out var massData1, 1.0f);

            FP.Abs(massData1.Center.X - center.X).ShouldBeLessThan(absTol + relTol * FP.Abs(center.X));
            FP.Abs(massData1.Center.Y - center.Y).ShouldBeLessThan(absTol + relTol * FP.Abs(center.Y));
            FP.Abs(massData1.Mass - mass).ShouldBeLessThan<FP>(20.0f * (absTol + relTol * mass));
            FP.Abs(massData1.RotationInertia - inertia).ShouldBeLessThan<FP>(40.0f * (absTol + relTol * inertia));

            polygon2.ComputeMass(out var massData2, 1.0f);

            FP.Abs(massData2.Center.X - center.X).ShouldBeLessThan(absTol + relTol * FP.Abs(center.X));
            FP.Abs(massData2.Center.Y - center.Y).ShouldBeLessThan(absTol + relTol * FP.Abs(center.Y));
            FP.Abs(massData2.Mass - mass).ShouldBeLessThan<FP>(20.0f * (absTol + relTol * mass));
            FP.Abs(massData2.RotationInertia - inertia).ShouldBeLessThan<FP>(40.0f * (absTol + relTol * inertia));
        }
    }
}