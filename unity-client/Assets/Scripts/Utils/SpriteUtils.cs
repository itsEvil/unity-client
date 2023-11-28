using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpriteUtils {
    public const int PIXELS_PER_UNIT = 8;
    public static readonly Vector2 Pivot = new(0.5f, 0);
    private static readonly Dictionary<Sprite, Dictionary<int, Sprite>> RedrawCache = new();
    private static readonly Dictionary<int, Dictionary<Color, Texture2D>> ColorCache = new();
    private static readonly Dictionary<int, Dictionary<Color, Sprite>> CircleCache = new();
    public static void ClearCache(CacheType type = CacheType.All) {
        if (type == CacheType.None)
            return;

        if (type == CacheType.All)
        {
            ClearRedraw();
            ClearColor();
            ClearCircle();
            return;
        }

        if (type == CacheType.Redraw)
        {
            ClearRedraw();
            return;
        }

        if (type == CacheType.Color)
        {
            ClearColor();
            return;
        }

        if (type == CacheType.Circle)
        {
            ClearCircle();
            return;
        }
    }

    private static void ClearCircle()
    {
        foreach (var dict in CircleCache.Values)
            dict.Clear();

        CircleCache.Clear();
    }

    private static void ClearColor()
    {
        foreach (var dict in ColorCache.Values)
            dict.Clear();

        ColorCache.Clear();
    }

    private static void ClearRedraw()
    {
        foreach (var dict in RedrawCache.Values)
            dict.Clear();

        RedrawCache.Clear();
    }

    public static Sprite Redraw(Sprite sprite, int size, float multiplier = 5)
    {
        var hash = GetHash(size, multiplier);
        if (IsCached(sprite, hash))
            return RedrawCache[sprite][hash];

        var scaledImage = TextureScaler.GetScaled(sprite, size, multiplier);
        Cache(sprite, hash, scaledImage);
        return scaledImage;
    }

    private static bool IsCached(Sprite sprite, int hash)
    {
        return RedrawCache.ContainsKey(sprite) && RedrawCache[sprite].ContainsKey(hash);
    }

    private static void Cache(Sprite original, int hash, Sprite modified)
    {
        if (!RedrawCache.ContainsKey(original))
            RedrawCache[original] = new Dictionary<int, Sprite>();

        RedrawCache[original][hash] = modified;
    }

    private static int GetHash(int size, float multiplier)
    {
        return (int)(size * multiplier);
    }


    /// <summary>
    /// Returns a circle sprite with FilterMode.Point from cache or creates it.
    /// </summary>
    /// <param name="color">color of the circle</param>
    /// <param name="radius">radius of the circle in unity units</param>
    /// <param name="multiplier">How detailed will the circle be</param>
    public static Sprite DrawCircle(Color color, float radius, int multiplier = 50)
    {
        float rad = radius * multiplier;

        if (IsCircleCached((int)rad, color))
        {
            return CircleCache[(int)rad][color];
        }

        Dictionary<Color, Sprite> dict = new Dictionary<Color, Sprite>();
        if (!CircleCache.TryGetValue((int)rad, out dict))
        {
            CircleCache[(int)rad] = new Dictionary<Color, Sprite>();
            dict = CircleCache[(int)rad];
        }


        int diameter = (int)rad * 2;

        Texture2D texture = new Texture2D(diameter, diameter);

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                Color clr = Color.clear;
                float dx = x - rad;
                float dy = y - rad;
                if (dx * dx + dy * dy <= rad * rad)
                {
                    clr = color;
                }
                texture.SetPixel(x, y, clr);
            }
        }

        texture.Apply();

        Vector2 pivot = new Vector2(0.5f, 0.5f);

        var sprite = Sprite.Create(texture, new Rect(0, 0, diameter, diameter), pivot, 1 * multiplier, 0, SpriteMeshType.FullRect);
        //sprite = TextureScaler.GetScaled(sprite, diameter, multiplier, pivot);


        return CircleCache[(int)rad][color] = sprite;
    }

    private static bool IsCircleCached(int size, Color color)
    {
        return CircleCache.ContainsKey(size) && CircleCache[size].ContainsKey(color);
    }

    public static Sprite RedrawSolidSquare(Color color, int size)
    {
        var colorDict = ColorCache[size];
        if (colorDict == null)
        {
            ColorCache[size] = new Dictionary<Color, Texture2D>();
            colorDict = ColorCache[size];
        }
        Texture2D tex = colorDict[color];
        if (tex != null)
        {
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.zero);
        }
        tex = new Texture2D(4, 4);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, color);
            }
        tex.Apply();

        ColorCache[size][color] = tex;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }

    public static Texture2D DrawSolidSquare(Color color, int width, int height)
    {
        var tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, color);
            }
        tex.Apply();

        return tex;
        //return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }
    public static Texture2D AddTexture(this Texture2D tex, Texture2D add)
    {
        tex.filterMode = FilterMode.Point;

        int startX = FPMathUtils.Random(0, tex.width - add.width);
        int startY = FPMathUtils.Random(0, tex.height - add.height);

        var pixels = add.GetPixels();
        int count = 0;
        for (int y = startY; y < startY + add.height; y++)
            for (int x = startX; x < startX + add.width; x++)
            {
                var color = pixels[count++];

                if (color.a == 0)
                    continue;

                tex.SetPixel(x, y, color);
            }
        tex.Apply();

        return tex;
        //return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }
    public static Color MostCommonColor(Sprite sprite)
    {
        var dict = new Dictionary<Color, int>();
        for (var x = (int)sprite.textureRect.x; x < sprite.textureRect.xMax; x++)
        {
            for (var y = (int)sprite.textureRect.y; y < sprite.textureRect.yMax; y++)
            {
                var color = sprite.texture.GetPixel(x, y);
                if (color == Color.clear)
                    continue;

                if (!dict.ContainsKey(color))
                    dict[color] = 1;
                else
                    dict[color]++;
            }
        }

        var bestColor = Color.black;
        var bestCount = 0;
        foreach (var colorPair in dict)
        {
            var color = colorPair.Key;
            var count = colorPair.Value;
            if (count > bestCount)
            {
                bestColor = color;
                bestCount = count;
            }
        }

        return bestColor;
    }
    public static void CopyPixels(this Texture2D to, Texture2D from, RectInt rect, Sprite alpha)
    {


        for (var y = rect.yMin; y < rect.yMax; y++)
        {
            for (var x = rect.xMin; x < rect.xMax; x++)
            {
                var toColor = to.GetPixel(x, y);
                var fromColor = from.GetPixel(x, y);
                var p = alpha.texture.GetPixel(x, y).a;
                var color = Color.Lerp(toColor, fromColor, p);
                to.SetPixel(x, y, color);
            }
        }

        to.Apply();
    }

    public static Sprite CreateSingleTextureSprite(Sprite sprite)
    {
        var texture = CreateTexture(sprite);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Pivot, PIXELS_PER_UNIT);
    }

    public static Sprite Rotate(Sprite sprite, int clockwiseTurns)
    {
        var rect = sprite.textureRect;
        var size = (int)rect.width;
        var colors = sprite.texture.GetPixels((int)rect.x, (int)rect.y, size, size);

        while (clockwiseTurns > 0)
        {
            var tempColors = colors.ToArray();
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    colors[x + y * size] = tempColors[y + (size - x - 1) * size];
                }
            }

            clockwiseTurns--;
        }

        while (clockwiseTurns < 0)
        {
            var tempColors = colors.ToArray();
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    colors[x + y * size] = tempColors[(size - y - 1) + x * size];

                }
            }

            clockwiseTurns++;
        }

        var texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Pivot, PIXELS_PER_UNIT);
    }

    public static Texture2D CreateTexture(Sprite sprite)
    {
        var smallTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, sprite.texture.format, false);
        smallTex.filterMode = FilterMode.Point;
        for (var y = 0; y < sprite.rect.height; y++)
        {
            for (var x = 0; x < sprite.rect.width; x++)
            {
                var color = sprite.texture.GetPixel((int)sprite.rect.x + x, (int)sprite.rect.y + y);
                smallTex.SetPixel(x, y, color);
            }
        }
        smallTex.Apply();
        return smallTex;
    }

    public static bool IsTransparent(this Sprite sprite)
    {
        for (var y = sprite.rect.y; y < sprite.rect.yMax; y++)
        {
            for (var x = sprite.rect.x; x < sprite.rect.xMax; x++)
            {
                var alpha = sprite.texture.GetPixel((int)x, (int)y).a;
                if (alpha > 0)
                    return false;
            }
        }

        return true;
    }

    public static bool IsTransparent(Texture2D texture, Rect rect)
    {
        for (var y = rect.y; y < rect.yMax; y++)
        {
            for (var x = rect.x; x < rect.xMax; x++)
            {
                var alpha = texture.GetPixel((int)x, (int)y).a;
                if (alpha > 0)
                    return false;
            }
        }

        return true;
    }

    public static Sprite Mirror(this Sprite sprite)
    {
        var width = (int)sprite.rect.width;
        var mirrored = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, sprite.texture.format, false);
        mirrored.filterMode = FilterMode.Point;
        for (var y = 0; y < sprite.rect.height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = sprite.texture.GetPixel((int)sprite.rect.x + x, (int)sprite.rect.y + y);
                mirrored.SetPixel(width - x - 1, y, color);
            }
        }
        mirrored.Apply();

        var rect = new Rect(0, 0, sprite.rect.width, sprite.rect.height);
        var pivot = Vector2.right - sprite.pivot / sprite.rect.width;
        var mirroredSprite = Sprite.Create(mirrored, rect, pivot, PIXELS_PER_UNIT);
        return mirroredSprite;
    }

    public static List<Sprite> CreateSprites(Texture2D texture, Rect targetRect, int imageWidth, int imageHeight)
    {
        List<Sprite> images = new();
        for (var y = targetRect.y - imageHeight; y >= targetRect.y - targetRect.height; y -= imageHeight)
        {
            for (var x = targetRect.x; x < targetRect.x + targetRect.width; x += imageWidth)
            {
                var rect = new Rect(x, y, imageWidth, imageHeight);
                var pivot = new Vector2(0.5f, 0);
                var sprite = Sprite.Create(texture, rect, pivot, PIXELS_PER_UNIT);

                images.Add(sprite);
            }
        }

        return images;
    }

    public static Sprite MergeSprites(Sprite first, Sprite second)
    {
        var rect = first.rect;
        rect.width += second.rect.width;
        var pivot = new Vector2(0.25f, 0);
        return Sprite.Create(first.texture, rect, pivot, PIXELS_PER_UNIT);
    }

    public enum CacheType
    {
        None,
        All,
        Color,
        Redraw,
        Circle,
    }
}

public static class TextureScaler
{
    /// <summary>
    /// Returns a scaled copy of given texture. 
    /// </summary>
    /// <param name="tex">Source texure to scale</param>
    /// <param name="width">Destination texture width</param>
    /// <param name="height">Destination texture height</param>
    /// <param name="mode">Filtering mode</param>
    public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
    {
        var texR = new Rect(0, 0, width, height);
        GpuScale(src, width, height, mode);

        //Get rendered data back to a new texture
        var result = new Texture2D(width, height, TextureFormat.ARGB32, true);
        result.Reinitialize(width, height);
        result.ReadPixels(texR, 0, 0, true);
        return result;
    }

    /// <summary>
    /// Scales the texture data of the given texture.
    /// </summary>
    /// <param name="tex">Texure to scale</param>
    /// <param name="width">New width</param>
    /// <param name="height">New height</param>
    /// <param name="mode">Filtering mode</param>
    public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
    {
        var texR = new Rect(0, 0, width, height);
        GpuScale(tex, width, height, mode);

        // Update new texture
        tex.Reinitialize(width, height);
        tex.ReadPixels(texR, 0, 0, true);
        tex.Apply(true); //Remove this if you hate us applying textures for you :)
    }
    public static Sprite GetScaled(Sprite sprite, int size, float multiplier, Vector2 pivot)
    {
        var rawTexture = sprite.texture;
        var smallTexture = new Texture2D((int)sprite.rect.width + 2, (int)sprite.rect.height + 2, TextureFormat.ARGB32, true);
        var rawPixels = rawTexture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);

        for (var i = 0; i < smallTexture.height; i++)
        {
            for (var j = 0; j < smallTexture.width; j++)
            {
                smallTexture.SetPixel(j, i, Color.clear);
            }
        }

        smallTexture.SetPixels(1, 1, (int)sprite.rect.width, (int)sprite.rect.height, rawPixels);
        smallTexture.Apply(true);

        var w = (int)(multiplier * size / 100 * smallTexture.width);
        var h = (int)(multiplier * size / 100 * smallTexture.height);
        Scale(smallTexture, w, h, FilterMode.Point);

        var rect = new Rect(0, 0, w, h);
        var rescaledSprite = Sprite.Create(smallTexture, rect, pivot, SpriteUtils.PIXELS_PER_UNIT * 6.25f);
        return rescaledSprite;
    }
    public static Sprite GetScaled(Sprite sprite, int size, float multiplier)
    {
        var rawTexture = sprite.texture;
        var smallTexture = new Texture2D((int)sprite.rect.width + 2, (int)sprite.rect.height + 2, TextureFormat.ARGB32, true);
        var rawPixels = rawTexture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);

        for (var i = 0; i < smallTexture.height; i++)
        {
            for (var j = 0; j < smallTexture.width; j++)
            {
                smallTexture.SetPixel(j, i, Color.clear);
            }
        }

        smallTexture.SetPixels(1, 1, (int)sprite.rect.width, (int)sprite.rect.height, rawPixels);
        smallTexture.Apply(true);

        var w = (int)(multiplier * size / 100 * smallTexture.width);
        var h = (int)(multiplier * size / 100 * smallTexture.height);
        var pivot = new Vector2(((sprite.pivot.x) / smallTexture.width), multiplier / h);
        Scale(smallTexture, w, h, FilterMode.Point);

        var rect = new Rect(0, 0, w, h);
        var rescaledSprite = Sprite.Create(smallTexture, rect, pivot, SpriteUtils.PIXELS_PER_UNIT * 6.25f);
        return rescaledSprite;
    }

    // Internal utility that renders the source texture into the RTT - the scaling method itself.
    private static void GpuScale(Texture2D src, int width, int height, FilterMode fmode)
    {
        //We need the source texture in VRAM because we render with it
        src.filterMode = fmode;
        src.Apply(true);

        //Using RTT for best quality and performance. Thanks, Unity 5
        var rtt = new RenderTexture(width, height, 32);

        //Set the RTT in order to render to it
        UnityEngine.Graphics.SetRenderTarget(rtt);

        //Setup 2D matrix in range 0..1, so nobody needs to care about sized
        GL.LoadPixelMatrix(0, 1, 1, 0);

        //Then clear & draw the texture to fill the entire RTT.
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        UnityEngine.Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
    }
}

