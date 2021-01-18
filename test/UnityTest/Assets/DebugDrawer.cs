using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Box2DSharp.Collision;
using Box2DSharp.Common;
using Box2DSharp.Testbed.Unity.Inspection;
using Testbed.Abstractions;
using UnityEngine;
using Color = Box2DSharp.Common.Color;
using Transform = Box2DSharp.Common.Transform;
using SVector2 = System.Numerics.Vector2;
using SVector3 = System.Numerics.Vector3;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;

namespace Box2DSharp.Testbed.Unity
{
    public class DebugDrawer : IDebugDrawer
    {
        public UnityDrawer Drawer;

        /// <inheritdoc />
        public DrawFlag Flags { get; set; }

        /// <inheritdoc />
        public void DrawPolygon(Span<FVector2> vertices, int vertexCount, in Color color)
        {
            Span<UVector2> ves = new UVector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                ves[i] = vertices[i].ToUVector2();
            }

            DrawPolygon(ves, vertexCount, color);
        }

        /// <inheritdoc />
        public void DrawSolidPolygon(Span<FVector2> vertices, int vertexCount, in Color color)
        {
            Span<UVector2> ves = new UVector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                ves[i] = vertices[i].ToUVector2();
            }

            DrawSolidPolygon(ves, vertexCount, color);
        }

        /// <inheritdoc />
        public void DrawCircle(in FVector2 center, FP radius, in Color color)
        {
            DrawCircle(center.ToVector2(), (float)radius, color);
        }

        /// <inheritdoc />
        public void DrawSolidCircle(in FVector2 center, FP radius, in FVector2 axis, in Color color)
        {
            DrawSolidCircle(center.ToUVector2(), (float)radius, axis.ToUVector2(), color);
        }

        /// <inheritdoc />
        public void DrawSegment(in FVector2 p1, in FVector2 p2, in Color color)
        {
            DrawSegment(p1.ToUVector2(), p2.ToUVector2(), color);
        }

        /// <inheritdoc />
        public void DrawPoint(in FVector2 p, FP size, in Color color)
        {
            DrawPoint(p.ToUVector2(), size.AsFloat, color);
        }

        public bool ShowUI = true;

        public void DrawPolygon(Span<UVector2> vertices, int vertexCount, in Color color)
        {
            var list = new List<(UVector3 begin, UVector3 end)>();

            for (var i = 0; i < vertexCount; i++)
            {
                if (i < vertexCount - 1)
                {
                    list.Add((vertices[i].ToUVector3(), vertices[i + 1].ToUVector3()));
                }
                else
                {
                    list.Add((vertices[i].ToUVector3(), vertices[0].ToUVector3()));
                }
            }

            Drawer.PostLines(list, color.ToUnityColor());
        }

        public void DrawSolidPolygon(Span<UVector2> vertices, int vertexCount, in Color color)
        {
            var list = new List<(UVector3 begin, UVector3 end)>();

            for (var i = 0; i < vertexCount; i++)
            {
                if (i < vertexCount - 1)
                {
                    list.Add((vertices[i].ToUVector3(), vertices[i + 1].ToUVector3()));
                }
                else
                {
                    list.Add((vertices[i].ToUVector3(), vertices[0].ToUVector3()));
                }
            }

            Drawer.PostLines(list, color.ToUnityColor());
        }

        public void DrawCircle(in SVector2 center, float radius, in Color color)
        {
            var lines = new List<(UVector3, UVector3)>();
            const int lineCount = 100;
            for (var i = 0; i <= lineCount; ++i) //割圆术画圆
            {
                lines.Add(
                    (
                        new UnityEngine.Vector2(
                            center.X + radius * (float)Math.Cos(2 * Mathf.PI / lineCount * i),
                            center.Y + radius * (float)Math.Sin(2 * Mathf.PI / lineCount * i)),
                        new UnityEngine.Vector2(
                            center.X + radius * (float)Math.Cos(2 * Mathf.PI / lineCount * (i + 1)),
                            center.Y + radius * (float)Math.Sin(2 * Mathf.PI / lineCount * (i + 1)))
                    ));
            }

            Drawer.PostLines(lines, color.ToUnityColor());
        }

        public void DrawSolidCircle(in UVector2 center, float radius, in UVector2 axis, in Color color)
        {
            var lines = new List<(UVector3, UVector3)>();
            const int lineCount = 100;
            for (var i = 0; i <= lineCount; ++i) //割圆术画圆
            {
                lines.Add(
                    (
                        new UnityEngine.Vector2(
                            center.x + radius * (float)Math.Cos(2 * Mathf.PI / lineCount * i),
                            center.y + radius * (float)Math.Sin(2 * Mathf.PI / lineCount * i)),
                        new UnityEngine.Vector2(
                            center.x + radius * (float)Math.Cos(2 * Mathf.PI / lineCount * (i + 1)),
                            center.y + radius * (float)Math.Sin(2 * Mathf.PI / lineCount * (i + 1)))
                    ));
            }

            Drawer.PostLines(lines, color.ToUnityColor());
            var p = center + radius * axis;
            DrawSegment(center, p, color);
        }

        public void DrawSegment(in UVector2 p1, in UVector2 p2, in Color color)
        {
            Drawer.PostLines(
                new List<(UVector3, UVector3)> {(p1.ToUVector3(), p2.ToUVector3())},
                color.ToUnityColor());
        }

        /// <inheritdoc />
        public void DrawTransform(in Transform xf)
        {
            const float axisScale = 0.4f;

            var p1 = xf.Position;
            var p2 = p1 + axisScale * xf.Rotation.GetXAxis();

            Drawer.PostLines(
                new List<(UVector3, UVector3)> {(p1.ToUVector2(), p2.ToUVector2())},
                UnityEngine.Color.red);

            p2 = p1 + axisScale * xf.Rotation.GetYAxis();
            Drawer.PostLines(
                new List<(UVector3 begin, UVector3 end)> {(p1.ToUVector2(), p2.ToUVector2())},
                UnityEngine.Color.green);
        }

        public void DrawPoint(in UVector2 p, float size, in Color color)
        {
            Drawer.PostPoint((p, size / 100, color.ToUnityColor()));
        }

        /// <inheritdoc />
        public void DrawAABB(AABB aabb, Color color)
        {
            var p1 = aabb.LowerBound.ToUVector2();
            var p2 = new UVector2(aabb.UpperBound.X.AsFloat, aabb.LowerBound.Y.AsFloat);
            var p3 = aabb.UpperBound.ToUVector2();
            var p4 = new UVector2(aabb.LowerBound.X.AsFloat, aabb.UpperBound.Y.AsFloat);
            DrawPolygon(new[] {p1, p2, p3, p4}, 4, color);
        }

        public readonly ConcurrentQueue<(SVector2 Position, string Text)> Texts = new ConcurrentQueue<(SVector2 Position, string Text)>();

        /// <inheritdoc />
        public void DrawString(float x, float y, string strings)
        {
            Texts.Enqueue((new SVector2(x, y), strings));
        }

        /// <inheritdoc />
        public void DrawString(int x, int y, string strings)
        {
            Texts.Enqueue((new SVector2(x, y), strings));
        }

        /// <inheritdoc />
        public void DrawString(SVector2 position, string strings)
        {
            Texts.Enqueue((position, strings));
        }
    }
}