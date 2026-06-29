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
        tex.SetPixels(pixels);

        bool[] conn = PieceConnections.GetBaseConnections(type);
        float thickness = TexSize * 0.22f;
        float center = TexSize * 0.5f;

        DrawDot(tex, (int)center, (int)center, thickness * 0.62f, PathColor);

        if (conn[(int)Direction.North]) DrawArm(tex, center, center, center, TexSize, thickness, PathColor);
        if (conn[(int)Direction.South]) DrawArm(tex, center, center, center, 0, thickness, PathColor);
        if (conn[(int)Direction.East]) DrawArm(tex, center, center, TexSize, center, thickness, PathColor);
        if (conn[(int)Direction.West]) DrawArm(tex, center, center, 0, center, thickness, PathColor);

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
