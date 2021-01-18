using System;

namespace Box2DSharp.Common
{
    public interface IDrawer
    {
        DrawFlag Flags { get; set; }

        /// Draw a closed polygon provided in CCW order.
        void DrawPolygon(Span<FVector2> vertices, int vertexCount, in Color color);

        /// Draw a solid closed polygon provided in CCW order.
        void DrawSolidPolygon(Span<FVector2> vertices, int vertexCount, in Color color);

        /// Draw a circle.
        void DrawCircle(in FVector2 center, FP radius, in Color color);

        /// Draw a solid circle.
        void DrawSolidCircle(in FVector2 center, FP radius, in FVector2 axis, in Color color);

        /// Draw a line segment.
        void DrawSegment(in FVector2 p1, in FVector2 p2, in Color color);

        /// Draw a transform. Choose your own length scale.
        /// @param xf a transform.
        void DrawTransform(in Transform xf);

        /// Draw a point.
        void DrawPoint(in FVector2 p, FP size, in Color color);
    }
}