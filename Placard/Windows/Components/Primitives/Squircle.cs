using Dalamud.Bindings.ImGui;

namespace Placard.Windows.Components;

internal static class Squircle
{
    private const float Exponent = 4.2f;
    private const int MinCornerSegments = 6;
    private const int MaxCornerSegments = 24;
    private const float SegmentError = 0.25f;
    private const float MinSegmentSquared = 0.01f;
    private const float DegenerateBox = 0.5f;

    private static readonly Vector2[][] UnitCorners = BuildUnitCorners();
    private static readonly Vector2[] PathScratch = new Vector2[(MaxCornerSegments + 1) * 4 + 4];

    public static void Fill(ImDrawListPtr drawList, Vector2 min, Vector2 max, float radius, uint color)
    {
        var box = CornerBox(min, max, radius);
        if (box <= DegenerateBox)
        {
            drawList.AddRectFilled(min, max, color);
            return;
        }

        TracePath(drawList, min, max, box);
        drawList.PathFillConvex(color);
    }

    public static void Stroke(ImDrawListPtr drawList, Vector2 min, Vector2 max, float radius, uint color,
        float thickness)
    {
        var box = CornerBox(min, max, radius);
        if (box <= DegenerateBox)
        {
            drawList.AddRect(min, max, color, 0f, ImDrawFlags.None, thickness);
            return;
        }

        TracePath(drawList, min, max, box);
        drawList.PathStroke(color, ImDrawFlags.Closed, thickness);
    }

    public static float CornerBox(Vector2 min, Vector2 max, float radius)
    {
        var limit = MathF.Min(max.X - min.X, max.Y - min.Y) * 0.5f;
        return MathF.Max(0f, MathF.Min(radius, limit));
    }

    private static void TracePath(ImDrawListPtr drawList, Vector2 min, Vector2 max, float box)
    {
        var count = 0;
        AppendCorner(new Vector2(min.X + box, min.Y + box), -1f, -1f, box, false, ref count);
        AppendCorner(new Vector2(max.X - box, min.Y + box), 1f, -1f, box, true, ref count);
        AppendCorner(new Vector2(max.X - box, max.Y - box), 1f, 1f, box, false, ref count);
        AppendCorner(new Vector2(min.X + box, max.Y - box), -1f, 1f, box, true, ref count);
        EmitScratch(drawList, count);
    }

    private static void AppendCorner(Vector2 anchor, float signX, float signY, float box, bool reverse, ref int count)
    {
        var corner = CornerFor(box);
        if (reverse)
        {
            for (var index = corner.Length - 1; index >= 0; index--)
            {
                var point = corner[index];
                AppendDistinct(new Vector2(anchor.X + signX * point.X * box, anchor.Y + signY * point.Y * box),
                    ref count);
            }

            return;
        }

        for (var index = 0; index < corner.Length; index++)
        {
            var point = corner[index];
            AppendDistinct(new Vector2(anchor.X + signX * point.X * box, anchor.Y + signY * point.Y * box), ref count);
        }
    }

    private static void AppendDistinct(Vector2 point, ref int count)
    {
        if (count > 0 && Vector2.DistanceSquared(PathScratch[count - 1], point) < MinSegmentSquared)
        {
            return;
        }

        PathScratch[count] = point;
        count++;
    }

    private static void EmitScratch(ImDrawListPtr drawList, int count)
    {
        while (count > 1 && Vector2.DistanceSquared(PathScratch[count - 1], PathScratch[0]) < MinSegmentSquared)
        {
            count--;
        }

        drawList.PathClear();
        for (var index = 0; index < count; index++)
        {
            drawList.PathLineTo(PathScratch[index]);
        }
    }

    private static Vector2[] CornerFor(float box) => UnitCorners[SegmentsFor(box) - MinCornerSegments];

    private static int SegmentsFor(float box)
    {
        if (box <= 1f)
        {
            return MinCornerSegments;
        }

        var cosine = MathF.Max(1f - SegmentError / box, -1f);
        var count = (int)MathF.Ceiling(MathF.PI * 0.5f / MathF.Acos(cosine));
        return Math.Clamp(count, MinCornerSegments, MaxCornerSegments);
    }

    private static Vector2[][] BuildUnitCorners()
    {
        var table = new Vector2[MaxCornerSegments - MinCornerSegments + 1][];
        for (var segments = MinCornerSegments; segments <= MaxCornerSegments; segments++)
        {
            table[segments - MinCornerSegments] = BuildUnitCorner(segments);
        }

        return table;
    }

    private static Vector2[] BuildUnitCorner(int segments)
    {
        var points = new Vector2[segments + 1];
        var power = 2f / Exponent;
        for (var index = 0; index <= segments; index++)
        {
            var angle = MathF.PI * 0.5f * index / segments;
            var cosine = MathF.Max(MathF.Cos(angle), 0f);
            var sine = MathF.Max(MathF.Sin(angle), 0f);
            points[index] = new Vector2(MathF.Pow(cosine, power), MathF.Pow(sine, power));
        }

        return points;
    }
}
