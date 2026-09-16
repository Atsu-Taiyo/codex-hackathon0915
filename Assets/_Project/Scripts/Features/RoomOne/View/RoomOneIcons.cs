using System.Collections.Generic;
using UnityEngine;

namespace Hackathon.RoomOne
{
    // Small antialiased masks generated from geometry; no OS font or glyph fallback.
    internal static class RoomOneIcons
    {
        static readonly Dictionary<string, Texture2D> masks = new Dictionary<string, Texture2D>();
        public static bool Draw(Rect rect, string symbol, float size, Color color)
        {
            if (symbol != "▶" && symbol != "⌂" && symbol != "★" && symbol != "↺" &&
                symbol != "⋮" && symbol != "›" && symbol != "↑" && symbol != "→" && symbol != "•••") return false;
            if (Event.current.type != EventType.Repaint) return true;
            if (!masks.TryGetValue(symbol, out var texture))
            {
                const int n = 96;
                texture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "UI icon " + symbol, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var pixels = new Color[n * n];
                for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                {
                    int coverage = 0;
                    for (int sy = 0; sy < 2; sy++) for (int sx = 0; sx < 2; sx++)
                        if (Inside(symbol, new Vector2((x + (sx + .5f) / 2) / n, (y + (sy + .5f) / 2) / n))) coverage++;
                    pixels[y * n + x] = new Color(1, 1, 1, coverage / 4f);
                }
                texture.SetPixels(pixels); texture.Apply(false, true); masks.Add(symbol, texture);
            }
            float side = Mathf.Min(size, Mathf.Min(rect.width, rect.height));
            GUI.DrawTexture(new Rect(rect.center.x - side / 2, rect.center.y - side / 2, side, side), texture, ScaleMode.StretchToFill, true, 0, color, 0, 0);
            return true;
        }
        static bool Line(Vector2 p, Vector2 a, Vector2 b, float width = .10f)
        {
            var d = b - a;
            return Vector2.Distance(p, a + d * Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude)) < width / 2;
        }
        static bool Inside(string s, Vector2 p)
        {
            float x = p.x, y = p.y;
            switch (s)
            {
                case "▶": return x > .2f && x < .85f && Mathf.Abs(y - .5f) < (.85f - x) * .62f;
                case "⌂": return Line(p,new Vector2(.12f,.52f),new Vector2(.5f,.88f)) || Line(p,new Vector2(.5f,.88f),new Vector2(.88f,.52f)) || (x > .23f && x < .77f && y > .13f && y < .55f && !(x > .43f && x < .59f && y < .40f));
                case "★":
                    bool hit = false;
                    Vector2 prev = StarPoint(9);
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 next = StarPoint(i);
                        if ((next.y > y) != (prev.y > y) && x < (prev.x-next.x)*(y-next.y)/(prev.y-next.y)+next.x) hit = !hit;
                        prev = next;
                    }
                    return hit;
                case "↺":
                    float radius = Vector2.Distance(p, new Vector2(.5f,.5f));
                    return (radius > .27f && radius < .39f && !(x < .3f && y > .5f && y < .73f)) || (x > .10f && x < .43f && y > .59f && y < .85f && y < .59f + (x-.10f));
                case "⋮": return Dot(p,.5f,.22f) || Dot(p,.5f,.5f) || Dot(p,.5f,.78f);
                case "•••": return Dot(p,.2f,.5f) || Dot(p,.5f,.5f) || Dot(p,.8f,.5f);
                case "›": return Line(p,new Vector2(.35f,.2f),new Vector2(.65f,.5f)) || Line(p,new Vector2(.65f,.5f),new Vector2(.35f,.8f));
                case "↑": p = new Vector2(p.y,1-p.x); goto case "→";
                case "→": return Line(p,new Vector2(.15f,.5f),new Vector2(.85f,.5f)) || Line(p,new Vector2(.6f,.25f),new Vector2(.85f,.5f)) || Line(p,new Vector2(.85f,.5f),new Vector2(.6f,.75f));
            }
            return false;
        }
        static bool Dot(Vector2 p, float x, float y) => Vector2.Distance(p,new Vector2(x,y)) < .065f;
        static Vector2 StarPoint(int i)
        {
            float angle = Mathf.PI / 2 + i * Mathf.PI / 5;
            return new Vector2(.5f,.5f) + new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)) * (i % 2 == 0 ? .46f : .20f);
        }
    }
}
