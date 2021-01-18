using Box2DSharp.Collision.Collider;
using Box2DSharp.Common;

namespace Box2DSharp.Collision.Shapes
{
    /// <summary>
    /// A line segment (edge) shape. These can be connected in chains or loops
    /// to other edge shapes. Edges created independently are two-sided and do
    /// no provide smooth movement across junctions. 
    /// </summary>
    public class EdgeShape : Shape
    {
        /// These are the edge vertices
        public FVector2 Vertex1;

        /// These are the edge vertices
        public FVector2 Vertex2;

        /// Optional adjacent vertices. These are used for smooth collision.
        public FVector2 Vertex0;

        /// Optional adjacent vertices. These are used for smooth collision.
        public FVector2 Vertex3;

        /// Uses m_vertex0 and m_vertex3 to create smooth collision.
        public bool OneSided;

        public EdgeShape()
        {
            ShapeType = ShapeType.Edge;
            Radius = Settings.PolygonRadius;
        }

        /// <summary>
        /// Set this as a part of a sequence. Vertex v0 precedes the edge and vertex v3
        /// follows. These extra vertices are used to provide smooth movement
        /// across junctions. This also makes the collision one-sided. The edge
        /// normal points to the right looking from v1 to v2.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public void SetOneSided(in FVector2 v0, in FVector2 v1, in FVector2 v2, in FVector2 v3)
        {
            Vertex0 = v0;
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
            OneSided = true;
        }

        /// <summary>
        /// Set this as an isolated edge. Collision is two-sided.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public void SetTwoSided(in FVector2 v1, in FVector2 v2)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            OneSided = false;
        }

        /// <inheritdoc />
        public override Shape Clone()
        {
            var clone = new EdgeShape
            {
                Vertex0 = Vertex0,
                Vertex1 = Vertex1,
                Vertex2 = Vertex2,
                Vertex3 = Vertex3,
                OneSided = OneSided
            };
            return clone;
        }

        /// <inheritdoc />
        public override int GetChildCount()
        {
            return 1;
        }

        /// <inheritdoc />
        public override bool TestPoint(in Transform transform, in FVector2 point)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool RayCast(
            out RayCastOutput output,
            in RayCastInput input,
            in Transform transform,
            int childIndex)
        {
            output = default;

            // Put the ray into the edge's frame of reference.
            var p1 = MathUtils.MulT(transform.Rotation, input.P1 - transform.Position);
            var p2 = MathUtils.MulT(transform.Rotation, input.P2 - transform.Position);
            var d = p2 - p1;

            var v1 = Vertex1;
            var v2 = Vertex2;
            var e = v2 - v1;

            // Normal points to the right, looking from v1 at v2
            var normal = new FVector2(e.Y, -e.X);
            normal.Normalize();

            // q = p1 + t * d
            // dot(normal, q - v1) = 0
            // dot(normal, p1 - v1) + t * dot(normal, d) = 0
            var numerator = FVector2.Dot(normal, v1 - p1);
            if (OneSided && numerator > FP.Zero)
            {
                return false;
            }

            var denominator = FVector2.Dot(normal, d);

            if (FP.Abs(denominator) < Settings.Epsilon)
            {
                return false;
            }

            var t = numerator / denominator;
            if (t < FP.Zero || input.MaxFraction < t)
            {
                return false;
            }

            var q = p1 + t * d;

            // q = v1 + s * r
            // s = dot(q - v1, r) / dot(r, r)
            var r = v2 - v1;
            var rr = FVector2.Dot(r, r);
            if (FP.Abs(rr) < Settings.Epsilon)
            {
                return false;
            }

            var s = FVector2.Dot(q - v1, r) / rr;
            if (s < FP.Zero || FP.One < s)
            {
                return false;
            }

            output = new RayCastOutput
            {
                Fraction = t,
                Normal = numerator > FP.Zero
                             ? -MathUtils.Mul(transform.Rotation, normal)
                             : MathUtils.Mul(transform.Rotation, normal)
            };

            return true;
        }

        /// <inheritdoc />
        public override void ComputeAABB(out AABB aabb, in Transform xf, int childIndex)
        {
            var v1 = MathUtils.Mul(xf, Vertex1);
            var v2 = MathUtils.Mul(xf, Vertex2);

            var lower = FVector2.Min(v1, v2);
            var upper = FVector2.Max(v1, v2);

            var r = new FVector2(Radius, Radius);
            aabb = new AABB(lower - r, upper + r);
        }

        /// <inheritdoc />
        public override void ComputeMass(out MassData massData, FP density)
        {
            massData = new MassData
            {
                Mass = 0,
                Center = 0.5 * (Vertex1 + Vertex2),
                RotationInertia = 0
            };
        }
    }
}