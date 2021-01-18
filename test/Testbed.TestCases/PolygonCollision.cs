using Box2DSharp.Collision;
using Box2DSharp.Collision.Collider;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Testbed.Abstractions;
using Color = Box2DSharp.Common.Color;
using Transform = Box2DSharp.Common.Transform;

namespace Testbed.TestCases
{
    [TestCase("Geometry", "Polygon Collision")]
    public class PolygonCollision : TestBase
    {
        private FP _angleB;

        private PolygonShape _polygonA = new PolygonShape();

        private PolygonShape _polygonB = new PolygonShape();

        private FVector2 _positionB;

        private Transform _transformA;

        private Transform _transformB;

        public PolygonCollision()
        {
            {
                _polygonA.SetAsBox(0.2f, 0.4f);
                _transformA.Set(new FVector2(0.0f, 0.0f), 0.0f);
            }

            {
                _polygonB.SetAsBox(0.5f, 0.5f);
                _positionB.Set(19.345284f, 1.5632932f);
                _angleB = 1.9160721f;
                _transformB.Set(_positionB, _angleB);
            }
        }

        /// <inheritdoc />
        public override void OnKeyDown(KeyInputEventArgs keyInput)
        {
            if (keyInput.Key == KeyCodes.A)
            {
                _positionB.X -= 0.1f;
            }

            if (keyInput.Key == KeyCodes.D)
            {
                _positionB.X += 0.1f;
            }

            if (keyInput.Key == KeyCodes.S)
            {
                _positionB.Y -= 0.1f;
            }

            if (keyInput.Key == KeyCodes.W)
            {
                _positionB.Y += 0.1f;
            }

            if (keyInput.Key == KeyCodes.Q)
            {
                _angleB += 0.1f * Settings.Pi;
            }

            if (keyInput.Key == KeyCodes.E)
            {
                _angleB -= 0.1f * Settings.Pi;
            }

            _transformB.Set(_positionB, _angleB);
        }

        private Manifold _manifold;

        private WorldManifold _worldManifold;

        /// <inheritdoc />
        protected override void PostStep()
        {
            _manifold = new Manifold();
            CollisionUtils.CollidePolygons(ref _manifold, _polygonA, _transformA, _polygonB, _transformB);
            _worldManifold = new WorldManifold();
            _worldManifold.Initialize(_manifold, _transformA, _polygonA.Radius, _transformB, _polygonB.Radius);
        }

        /// <inheritdoc />
        protected override void OnGUI()
        {
            DrawString($"point count = {_manifold.PointCount}");
        }

        protected override void OnRender()
        {
            var color = Color.FromArgb(230, 230, 230);
            var v = new FVector2[Settings.MaxPolygonVertices];
            for (var i = 0; i < _polygonA.Count; ++i)
            {
                v[i] = MathUtils.Mul(_transformA, _polygonA.Vertices[i]);
            }

            Drawer.DrawPolygon(v, _polygonA.Count, color);

            for (var i = 0; i < _polygonB.Count; ++i)
            {
                v[i] = MathUtils.Mul(_transformB, _polygonB.Vertices[i]);
            }

            Drawer.DrawPolygon(v, _polygonB.Count, color);

            for (var i = 0; i < _manifold.PointCount; ++i)
            {
                Drawer.DrawPoint(_worldManifold.Points[i], 4.0f, Color.FromArgb(230, 77, 77));
            }
        }
    }
}