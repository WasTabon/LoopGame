using System.Collections.Generic;
using UnityEngine;

public static class PieceSpriteFactory
{
    private const int TexSize = 256;
    private const float PixelsPerUnit = 256f;

    private static readonly Dictionary<PieceType, Sprite> cache = new Dictionary<PieceType, Sprite>();
    private static Sprite cellSprite;
    private static Sprite obstacleSprite;
    private static Sprite startSprite;
    private static Sprite glowDotSprite;
    private static Sprite rotationArrowSprite;
    private static Sprite tapPointerSprite;
    private static Sprite starFilledSprite;
    private static Sprite starEmptySprite;
    private static Sprite lockSprite;

    private static readonly Color PathColor = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color StartColor = new Color(0.961f, 0.651f, 0.137f, 1f);

    public static Sprite GetPieceSprite(PieceType type)
    {
        if (cache.TryGetValue(type, out Sprite cached)) return cached;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TexSize * TexSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        bool[] conn = PieceConnections.GetBaseConnections(type);
        float thickness = TexSize * 0.24f;
        float center = TexSize * 0.5f;
        float half = thickness * 0.5f;

        Color baseCol = PathColor;
        Color highlightCol = new Color(
            Mathf.Min(1f, PathColor.r + 0.22f),
            Mathf.Min(1f, PathColor.g + 0.20f),
            Mathf.Min(1f, PathColor.b + 0.16f), 1f);

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                float d = float.MaxValue;
                d = Mathf.Min(d, DistanceToSegment(x, y, center, center, center, center));
                if (conn[(int)Direction.North]) d = Mathf.Min(d, DistanceToSegment(x, y, center, center, center, TexSize));
                if (conn[(int)Direction.South]) d = Mathf.Min(d, DistanceToSegment(x, y, center, center, center, 0));
                if (conn[(int)Direction.East]) d = Mathf.Min(d, DistanceToSegment(x, y, center, center, TexSize, center));
                if (conn[(int)Direction.West]) d = Mathf.Min(d, DistanceToSegment(x, y, center, center, 0, center));

                float edge = half - d;
                if (edge <= -1.5f) continue;

                float alpha = Mathf.Clamp01((edge + 1.5f) / 3f);

                float depth = Mathf.Clamp01(d / half);
                Color col = Color.Lerp(highlightCol, baseCol, depth);

                pixels[y * TexSize + x] = new Color(col.r, col.g, col.b, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        cache[type] = sprite;
        return sprite;
    }

    public static Sprite GetCellSprite()
    {
        if (cellSprite != null) return cellSprite;
        cellSprite = CreateRoundedSquare(new Color(1f, 1f, 1f, 1f), 0.18f);
        return cellSprite;
    }

    public static Sprite GetStartMarkerSprite()
    {
        if (startSprite != null) return startSprite;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TexSize * TexSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        tex.SetPixels(pixels);

        float center = TexSize * 0.5f;
        DrawRing(tex, center, center, TexSize * 0.34f, TexSize * 0.09f, StartColor);

        tex.Apply();
        startSprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        return startSprite;
    }

    public static Sprite GetGlowDotSprite()
    {
        if (glowDotSprite != null) return glowDotSprite;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float center = TexSize * 0.5f;
        float radius = TexSize * 0.5f;
        Color core = new Color(1f, 0.97f, 0.85f, 1f);

        Color[] pixels = new Color[TexSize * TexSize];
        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha;
                pixels[y * TexSize + x] = new Color(core.r, core.g, core.b, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        glowDotSprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        return glowDotSprite;
    }

    public static Sprite GetRotationArrowSprite()
    {
        if (rotationArrowSprite != null) return rotationArrowSprite;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TexSize * TexSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        float center = TexSize * 0.5f;
        float ringRadius = TexSize * 0.32f;
        float thickness = TexSize * 0.08f;
        Color arrowColor = new Color(0.961f, 0.651f, 0.137f, 1f);

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;

                bool onRing = Mathf.Abs(dist - ringRadius) < thickness;
                bool inArc = angle > 40f && angle < 320f;
                if (onRing && inArc)
                {
                    pixels[y * TexSize + x] = arrowColor;
                }
            }
        }

        DrawArrowHead(pixels, center, ringRadius, arrowColor);

        tex.SetPixels(pixels);
        tex.Apply();
        rotationArrowSprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        return rotationArrowSprite;
    }

    private static void DrawArrowHead(Color[] pixels, float center, float ringRadius, Color color)
    {
        float headAngle = 40f * Mathf.Deg2Rad;
        float hx = center + Mathf.Cos(headAngle) * ringRadius;
        float hy = center + Mathf.Sin(headAngle) * ringRadius;
        float headSize = TexSize * 0.14f;

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                float dx = x - hx;
                float dy = y - hy;
                if (Mathf.Sqrt(dx * dx + dy * dy) < headSize)
                {
                    pixels[y * TexSize + x] = color;
                }
            }
        }
    }

    public static Sprite GetTapPointerSprite()
    {
        if (tapPointerSprite != null) return tapPointerSprite;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TexSize * TexSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        float center = TexSize * 0.5f;
        float outerRing = TexSize * 0.42f;
        float ringWidth = TexSize * 0.05f;
        float innerDot = TexSize * 0.20f;
        Color white = new Color(1f, 1f, 1f, 0.95f);
        Color soft = new Color(1f, 1f, 1f, 0.55f);

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (Mathf.Abs(dist - outerRing) < ringWidth)
                {
                    pixels[y * TexSize + x] = soft;
                }
                else if (dist < innerDot)
                {
                    float a = Mathf.Clamp01(1f - dist / innerDot) * 0.4f + 0.6f;
                    pixels[y * TexSize + x] = new Color(white.r, white.g, white.b, a);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tapPointerSprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        return tapPointerSprite;
    }

    public static Sprite GetStarSprite(bool filled)
    {
        if (filled && starFilledSprite != null) return starFilledSprite;
        if (!filled && starEmptySprite != null) return starEmptySprite;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TexSize * TexSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        float center = TexSize * 0.5f;
        float outer = TexSize * 0.44f;
        float inner = outer * 0.42f;

        Vector2[] points = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float ang = Mathf.Deg2Rad * (90f + i * 36f);
            float r = (i % 2 == 0) ? outer : inner;
            points[i] = new Vector2(center + Mathf.Cos(ang) * r, center + Mathf.Sin(ang) * r);
        }

        Color fillCol = filled ? new Color(0.984f, 0.741f, 0.243f, 1f) : new Color(1f, 1f, 1f, 0.16f);
        Color edgeCol = filled ? new Color(0.961f, 0.561f, 0.110f, 1f) : new Color(1f, 1f, 1f, 0.22f);

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                if (PointInPolygon(x + 0.5f, y + 0.5f, points))
                {
                    float dEdge = DistanceToPolygonEdge(x + 0.5f, y + 0.5f, points);
                    Color c = dEdge < 6f ? edgeCol : fillCol;
                    float aa = Mathf.Clamp01(dEdge / 1.5f + 0.3f);
                    pixels[y * TexSize + x] = new Color(c.r, c.g, c.b, c.a * Mathf.Clamp01(aa));
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        if (filled) starFilledSprite = sprite; else starEmptySprite = sprite;
        return sprite;
    }

    public static Sprite GetLockSprite()
    {
        if (lockSprite != null) return lockSprite;

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TexSize * TexSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        Color body = new Color(0.85f, 0.85f, 0.92f, 0.9f);
        float cx = TexSize * 0.5f;

        float bodyLeft = TexSize * 0.30f;
        float bodyRight = TexSize * 0.70f;
        float bodyBottom = TexSize * 0.20f;
        float bodyTop = TexSize * 0.52f;
        float bodyRadius = TexSize * 0.06f;

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                bool inBody = IsInsideRoundedRectRegion(x, y, bodyLeft, bodyRight, bodyBottom, bodyTop, bodyRadius);
                if (inBody)
                {
                    pixels[y * TexSize + x] = body;
                }
            }
        }

        float shackleCenterY = bodyTop;
        float shackleOuter = TexSize * 0.16f;
        float shackleInner = TexSize * 0.10f;
        float shackleThickness = (shackleOuter - shackleInner);

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                float dx = x - cx;
                float dy = y - shackleCenterY;
                if (dy < 0) continue;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float ringCenter = (shackleOuter + shackleInner) * 0.5f;
                if (Mathf.Abs(dist - ringCenter) <= shackleThickness * 0.5f)
                {
                    pixels[y * TexSize + x] = body;
                }
            }
        }

        Color keyhole = new Color(0.25f, 0.25f, 0.32f, 1f);
        float khY = (bodyBottom + bodyTop) * 0.5f;
        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                float dx = x - cx;
                float dy = y - khY;
                if (dx * dx + dy * dy <= (TexSize * 0.045f) * (TexSize * 0.045f))
                {
                    pixels[y * TexSize + x] = keyhole;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        lockSprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
        return lockSprite;
    }

    private static bool IsInsideRoundedRectRegion(int x, int y, float left, float right, float bottom, float top, float radius)
    {
        float cx = Mathf.Clamp(x, left + radius, right - radius);
        float cy = Mathf.Clamp(y, bottom + radius, top - radius);
        if (x >= left + radius && x <= right - radius && y >= bottom && y <= top) return true;
        if (y >= bottom + radius && y <= top - radius && x >= left && x <= right) return true;
        float dx = x - cx;
        float dy = y - cy;
        return (dx * dx + dy * dy) <= radius * radius;
    }

    private static bool PointInPolygon(float px, float py, Vector2[] poly)
    {
        bool inside = false;
        int j = poly.Length - 1;
        for (int i = 0; i < poly.Length; i++)
        {
            if (((poly[i].y > py) != (poly[j].y > py)) &&
                (px < (poly[j].x - poly[i].x) * (py - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }

    private static float DistanceToPolygonEdge(float px, float py, Vector2[] poly)
    {
        float min = float.MaxValue;
        int j = poly.Length - 1;
        for (int i = 0; i < poly.Length; i++)
        {
            float d = DistanceToSegment(px, py, poly[j].x, poly[j].y, poly[i].x, poly[i].y);
            if (d < min) min = d;
            j = i;
        }
        return min;
    }

    private static Sprite CreateRoundedSquare(Color color, float cornerFraction)
    {
        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[TexSize * TexSize];
        float corner = TexSize * cornerFraction;

        for (int y = 0; y < TexSize; y++)
        {
            for (int x = 0; x < TexSize; x++)
            {
                if (IsInsideRoundedRect(x, y, TexSize, TexSize, corner))
                    pixels[y * TexSize + x] = color;
                else
                    pixels[y * TexSize + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize),
            new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static bool IsInsideRoundedRect(int x, int y, int w, int h, float radius)
    {
        float left = radius;
        float right = w - radius;
        float bottom = radius;
        float top = h - radius;

        float cx = Mathf.Clamp(x, left, right);
        float cy = Mathf.Clamp(y, bottom, top);

        float dx = x - cx;
        float dy = y - cy;

        return (dx * dx + dy * dy) <= (radius * radius);
    }

    private static void DrawArm(Texture2D tex, float x0, float y0, float x1, float y1, float thickness, Color color)
    {
        int minX = Mathf.Max(0, (int)(Mathf.Min(x0, x1) - thickness));
        int maxX = Mathf.Min(tex.width - 1, (int)(Mathf.Max(x0, x1) + thickness));
        int minY = Mathf.Max(0, (int)(Mathf.Min(y0, y1) - thickness));
        int maxY = Mathf.Min(tex.height - 1, (int)(Mathf.Max(y0, y1) + thickness));

        float half = thickness * 0.5f;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dist = DistanceToSegment(x, y, x0, y0, x1, y1);
                if (dist <= half)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void DrawDot(Texture2D tex, int cx, int cy, float radius, Color color)
    {
        int minX = Mathf.Max(0, (int)(cx - radius));
        int maxX = Mathf.Min(tex.width - 1, (int)(cx + radius));
        int minY = Mathf.Max(0, (int)(cy - radius));
        int maxY = Mathf.Min(tex.height - 1, (int)(cy + radius));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                if (dx * dx + dy * dy <= radius * radius)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void DrawRing(Texture2D tex, float cx, float cy, float radius, float thickness, Color color)
    {
        int minX = Mathf.Max(0, (int)(cx - radius - thickness));
        int maxX = Mathf.Min(tex.width - 1, (int)(cx + radius + thickness));
        int minY = Mathf.Max(0, (int)(cy - radius - thickness));
        int maxY = Mathf.Min(tex.height - 1, (int)(cy + radius + thickness));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (Mathf.Abs(dist - radius) <= thickness)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }
    }

    private static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by)
    {
        float abx = bx - ax;
        float aby = by - ay;
        float apx = px - ax;
        float apy = py - ay;

        float abLenSq = abx * abx + aby * aby;
        float t = abLenSq > 0f ? (apx * abx + apy * aby) / abLenSq : 0f;
        t = Mathf.Clamp01(t);

        float closestX = ax + t * abx;
        float closestY = ay + t * aby;

        float dx = px - closestX;
        float dy = py - closestY;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
