using Data;
using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileRedrawer
{
    private static byte[] _bytes = new byte[128];
    private static byte _byteIndex = 0;

    private static bool _created = false;
    private static Dictionary<object[], Sprite>[] _Caches = new Dictionary<object[], Sprite>[2];
    private static readonly Dictionary<ushort, Sprite> _TileCache = new(new TileCacheComparer());

    private static readonly Rect TextureRect8 = new(0, 0, 8, 8);
    private static readonly RectInt Rect0 = new(0, 0, 4, 4);
    private static readonly RectInt Rect1 = new(4, 0, 4, 4);
    private static readonly RectInt Rect2 = new(0, 4, 4, 4);
    private static readonly RectInt Rect3 = new(4, 4, 4, 4);

    private static readonly Vector2Int Point0 = new(0, 0);
    private static readonly Vector2Int Point1 = new(4, 0);
    private static readonly Vector2Int Point2 = new(0, 4);
    private static readonly Vector2Int Point3 = new(4, 4);

    private static readonly List<Sprite> _MaskLists = GetMasks();
    private const int _INNER = 0;
    private const int _SIDE0 = 1;
    private const int _SIDE1 = 2;
    private const int _OUTER = 3;
    private const int _INNER_P1 = 4;
    private const int _INNER_P2 = 5;
    static TileRedrawer() => Init();
    public static void Init()
    {
        if (_created)
            foreach (var cache in _Caches)
                cache.Clear();

        if (!_created)
            for (int i = 0; i < 2; i++)
            {
                _Caches[i] = new(new CacheComparer());
            }

        _created = true;

        for (int i = 0; i < _bytes.Length; i++) {
            _bytes[i] = (byte)FPMathUtils.Random(0, 2);
        }

        //Debug.Log("TileRedrawer::Init");
    }
    public static Sprite RedrawSigless(Square square, bool originalBackground)
    {
        if (_TileCache.TryGetValue(square.Type, out var sprite))
            return sprite;

        var orig = AssetLibrary.GetTileImage(square.Type);
        Texture2D texture;

        if (originalBackground)
        {
            texture = SpriteUtils.CreateTexture(orig);
        }
        else
        {
            texture = new Texture2D(8, 8);
            texture.filterMode = FilterMode.Point;
        }

        texture.Apply();
        sprite = Sprite.Create(texture, TextureRect8, SpriteUtils.Pivot, SpriteUtils.PIXELS_PER_UNIT);
        _TileCache[square.Type] = sprite;
        return sprite;

    }
    public static Sprite Redraw(Square square, bool originalBackground)
    {
        //Debug.Log($"Type: {square.Type}");
        object[] sig;
        if (square.Type == 253)
        {
            sig = GetCompositeSig(square);
        }
        else if (square.Desc.HasEdge)
        {
            sig = GetEdgeSig(square);
        }
        else
        {
            sig = GetSig(square);
        }

        //Debug.Log($"SigIsNull::{sig == null}");

        if (sig == null)
            return null;

        byte index = FPMathUtils.RandomRef(ref _bytes, ref _byteIndex);
        //Debug.Log($"CacheIndex:{index}::{sig}");
        var cache = _Caches[index];

        if (cache.TryGetValue(sig, out var sprite))
            return sprite;

        if (square.Type == 253)
        {
            //var newSprite = BuildComposite(sig);
            //_Cache[sig] = newSprite;
            return null;
            //return newSprite;
        }

        if (square.Desc.HasEdge)
        {
            //var newSprite = DrawEdges(sig);
            //_Cache[sig] = newSprite;
            return null;
            //return null;//newSprite;
        }

        var redraw0 = false;
        var redraw1 = false;
        var redraw2 = false;
        var redraw3 = false;
        var s0 = (ushort)sig[0];
        var s1 = (ushort)sig[1];
        var s2 = (ushort)sig[2];
        var s3 = (ushort)sig[3];
        var s4 = (ushort)sig[4];
        var s5 = (ushort)sig[5];
        var s6 = (ushort)sig[6];
        var s7 = (ushort)sig[7];
        var s8 = (ushort)sig[8];
        if (s1 != s4)
        {
            redraw0 = true;
            redraw1 = true;
        }
        if (s3 != s4)
        {
            redraw0 = true;
            redraw2 = true;
        }
        if (s5 != s4)
        {
            redraw1 = true;
            redraw3 = true;
        }
        if (s7 != s4)
        {
            redraw2 = true;
            redraw3 = true;
        }
        if (!redraw0 && s0 != s4)
        {
            redraw0 = true;
        }
        if (!redraw1 && s2 != s4)
        {
            redraw1 = true;
        }
        if (!redraw2 && s6 != s4)
        {
            redraw2 = true;
        }
        if (!redraw3 && s8 != s4)
        {
            redraw3 = true;
        }
        if (!redraw0 && !redraw1 && !redraw2 && !redraw3)
        {
            cache[sig] = null;
            _Caches[index] = cache;
            return null;
        }

        var orig = AssetLibrary.GetTileImage(square.Type);
        Texture2D texture;
        if (originalBackground)
        {
            texture = SpriteUtils.CreateTexture(orig);
        }
        else
        {
            texture = new Texture2D(8, 8);
            texture.filterMode = FilterMode.Point;
        }

        //Todo add masks
        //if (redraw0) {
        //    RedrawRect(texture, Rect0, _MaskLists[0], s4, s3, s0, s1);
        //}
        //if (redraw1) {
        //    RedrawRect(texture, Rect1, _MaskLists[1], s4, s1, s2, s5);
        //}
        //if (redraw2) {
        //    RedrawRect(texture, Rect2, _MaskLists[2], s4, s7, s6, s3);
        //}
        //if (redraw3) {
        //    RedrawRect(texture, Rect3, _MaskLists[3], s4, s5, s8, s7);
        //}

        texture.Apply();
        sprite = Sprite.Create(texture, TextureRect8, SpriteUtils.Pivot, SpriteUtils.PIXELS_PER_UNIT);

        cache[sig] = sprite;
        _Caches[index] = cache;
        //_Cache[sig] = sprite;
        return sprite;
    }

    private static void RedrawRect(Texture2D texture, RectInt rect, List<List<Sprite>> masks, ushort b, ushort n0, ushort n1, ushort n2) {
        Sprite mask;
        Sprite blend;
        if (b == n0 && b == n2) {
            mask = masks[_OUTER].Random();
            blend = AssetLibrary.GetTileImage(n1);
        }
        else if (b != n0 && b != n2)
        {
            if (n0 != n2)
            {
                var n0Image = SpriteUtils.CreateTexture(AssetLibrary.GetTileImage(n0));
                var n2Image = SpriteUtils.CreateTexture(AssetLibrary.GetTileImage(n2));
                texture.CopyPixels(n0Image, rect, masks[_INNER_P2].Random());
                texture.CopyPixels(n2Image, rect, masks[_INNER_P1].Random());
                return;
            }

            mask = masks[_INNER].Random();
            blend = AssetLibrary.GetTileImage(n0);
        }
        else if (b != n0)
        {
            mask = masks[_SIDE0].Random();
            blend = AssetLibrary.GetTileImage(n0);
        }
        else
        {
            mask = masks[_SIDE1].Random();
            blend = AssetLibrary.GetTileImage(n2);
        }

        var blendTex = SpriteUtils.CreateTexture(blend);
        texture.CopyPixels(blendTex, rect, mask);
    }

    private static List<Sprite> GetMasks() {
        return AssetLibrary.GetImageSet("GroundMasks");
    }

    private static void AddMasks(List<List<List<Sprite>>> list, List<Sprite> inner, List<Sprite> side, List<Sprite> outer,
        List<Sprite> innerP1, List<Sprite> innerP2)
    {
        foreach (var i in new[] { -1, 0, 2, 1 })
        {
            list.Add(new List<List<Sprite>>()
            {
                RotateImageSet(inner, i - 1), RotateImageSet(side, i - 1), RotateImageSet(side, i),
                RotateImageSet(outer, i - 1), RotateImageSet(innerP1, i - 1), RotateImageSet(innerP2, i - 1)
            });
        }
    }

    private static List<Sprite> RotateImageSet(List<Sprite> sprites, int clockwiseTurns)
    {
        var rotatedSprites = new List<Sprite>();
        foreach (var sprite in sprites)
        {
            rotatedSprites.Add(SpriteUtils.Rotate(sprite, clockwiseTurns));
        }

        return rotatedSprites;
    }

    private static Sprite DrawEdges(object[] sig)
    {
        var orig = AssetLibrary.GetTileImage((ushort)sig[4]);
        var texture = SpriteUtils.CreateTexture(orig);
        var desc = AssetLibrary.GetTileDesc((ushort)sig[4]);
        var edges = desc.GetEdges();
        var innerCorners = desc.GetInnerCorners();
        for (var i = 1; i < 8; i += 2)
        {
            if (!(bool)sig[i])
            {
                texture.SetPixels32(edges[i].texture.GetPixels32());
            }
        }

        var s0 = (bool)sig[0];
        var s1 = (bool)sig[1];
        var s2 = (bool)sig[2];
        var s3 = (bool)sig[3];
        var s5 = (bool)sig[5];
        var s6 = (bool)sig[6];
        var s7 = (bool)sig[7];
        var s8 = (bool)sig[8];
        if (edges[0] != null)
        {
            if (s3 && s1 && !s0)
            {
                texture.SetPixels32(edges[0].texture.GetPixels32());
            }
            if (s1 && s5 && !s2)
            {
                texture.SetPixels32(edges[2].texture.GetPixels32());
            }
            if (s5 && s7 && !s8)
            {
                texture.SetPixels32(edges[8].texture.GetPixels32());
            }
            if (s3 && s7 && !s6)
            {
                texture.SetPixels32(edges[6].texture.GetPixels32());
            }
        }

        if (innerCorners != null)
        {
            if (!s3 && !s1)
            {
                texture.SetPixels32(innerCorners[0].texture.GetPixels32());
            }
            if (!s1 && !s5)
            {
                texture.SetPixels32(innerCorners[2].texture.GetPixels32());
            }
            if (!s5 && !s7)
            {
                texture.SetPixels32(innerCorners[8].texture.GetPixels32());
            }
            if (!s7 && !s3)
            {
                texture.SetPixels32(innerCorners[6].texture.GetPixels32());
            }
        }

        texture.Apply();
        return Sprite.Create(texture, TextureRect8, SpriteUtils.Pivot, SpriteUtils.PIXELS_PER_UNIT);
    }

    private static Sprite BuildComposite(object[] sig)
    {
        var texture = new Texture2D((int)TextureRect8.width, (int)TextureRect8.height);
        texture.filterMode = FilterMode.Point;
        var s0 = (int)sig[0];
        var s1 = (int)sig[1];
        var s2 = (int)sig[2];
        var s3 = (int)sig[3];
        if (s0 != 255)
        {
            var neighbor = AssetLibrary.GetTileImage((ushort)s0);
            var pixels = neighbor.texture.GetPixels(Point0.x, Point0.y, Rect0.width, Rect0.height);
            texture.SetPixels(Point0.x, Point0.y, Rect0.width, Rect0.height, pixels);
        }
        if (s1 != 255)
        {
            var neighbor = AssetLibrary.GetTileImage((ushort)s1);
            var pixels = neighbor.texture.GetPixels(Point1.x, Point1.y, Rect1.width, Rect1.height);
            texture.SetPixels(Point1.x, Point1.y, Rect1.width, Rect1.height, pixels);
        }
        if (s2 != 255)
        {
            var neighbor = AssetLibrary.GetTileImage((ushort)s2);
            var pixels = neighbor.texture.GetPixels(Point2.x, Point2.y, Rect2.width, Rect2.height);
            texture.SetPixels(Point2.x, Point2.y, Rect2.width, Rect2.height, pixels);
        }
        if (s3 != 255)
        {
            var neighbor = AssetLibrary.GetTileImage((ushort)s3);
            var pixels = neighbor.texture.GetPixels(Point3.x, Point3.y, Rect3.width, Rect3.height);
            texture.SetPixels(Point3.x, Point3.y, Rect3.width, Rect3.height, pixels);
        }
        texture.Apply();
        return Sprite.Create(texture, TextureRect8, SpriteUtils.Pivot, SpriteUtils.PIXELS_PER_UNIT);
    }

    private static object[] GetSig(Square square)
    {
        var width = Map.MapInfo.Width;
        var height = Map.MapInfo.Height;

        object[] sigs = new object[9];
        int index = -1;
        //var sig = new List<object>();
        var map = Map.Instance;
        for (var y = square.Y - 1; y <= square.Y + 1; y++)
        {
            for (var x = square.X - 1; x <= square.X + 1; x++)
            {
                index++;
                if (x < 0 || x >= width || y < 0 || y >= height ||
                    x == square.X && y == square.Y)
                {
                    sigs[index] = square.Type;
                    //sig.Add(square.Type);
                    continue;
                }

                var n = map.GetTile(x, y);
                if (n == null || n.Desc.BlendPriority <= square.Desc.BlendPriority)
                {
                    sigs[index] = square.Type;
                    //sig.Add(square.Type);
                    continue;
                }
                sigs[index] = n.Type;
                //sig.Add(n.Type);
            }
        }
        return sigs;
        //return sig.ToArray();
    }

    private static object[] GetEdgeSig(Square square)
    {
        var width = Map.MapInfo.Width;
        var height = Map.MapInfo.Height;

        var sig = new List<object>();
        var hasEdge = false;
        var sameTypeEdgeMode = square.Desc.SameTypeEdgeMode;
        for (var y = square.Y - 1; y <= square.Y + 1; y++)
        {
            for (var x = square.X - 1; x <= square.X + 1; x++)
            {
                var n = Map.Instance.GetTile(x, y);

                if (n == null) continue;

                if (x == square.X && y == square.Y)
                {
                    sig.Add(n.Type);
                    continue;
                }

                bool b;
                if (sameTypeEdgeMode)
                {
                    b = n == null || n.Type == square.Type;
                }
                else
                {
                    b = n == null || n.Type != 255;
                }
                sig.Add(b);
                hasEdge = hasEdge || !b;
            }
        }

        return hasEdge ? sig.ToArray() : null;
    }

    private static object[] GetCompositeSig(Square square)
    {
        var map = Map.Instance;
        var sig = new object[4];
        var x = square.X;
        var y = square.Y;
        var n1 = map.GetTile(x, y - 1);
        var n2 = map.GetTile(x - 1, y);
        var n3 = map.GetTile(x + 1, y);
        var n4 = map.GetTile(x, y + 1);
        var p1 = n1 != null ? n1.Desc.CompositePriority : -1;
        var p2 = n2 != null ? n2.Desc.CompositePriority : -1;
        var p3 = n3 != null ? n3.Desc.CompositePriority : -1;
        var p4 = n4 != null ? n4.Desc.CompositePriority : -1;
        if (p1 < 0 && p2 < 0)
        {
            var n0 = map.GetTile(x - 1, y - 1);
            sig[0] = n0 == null || n0.Desc.CompositePriority < 0 ? 255 : n0.Type;
        }
        else if (p1 < p2)
        {
            sig[0] = n2.Type;
        }
        else
        {
            sig[0] = n1.Type;
        }

        if (p1 < 0 && p3 < 0)
        {
            var n0 = map.GetTile(x + 1, y - 1);
            sig[1] = n0 == null || n0.Desc.CompositePriority < 0 ? 255 : n0.Type;
        }
        else if (p1 < p3)
        {
            sig[1] = n3.Type;
        }
        else
        {
            sig[1] = n1.Type;
        }

        if (p2 < 0 && p4 < 0)
        {
            var n0 = map.GetTile(x - 1, y + 1);
            sig[2] = n0 == null || n0.Desc.CompositePriority < 0 ? 255 : n0.Type;
        }
        else if (p2 < p4)
        {
            sig[2] = n4.Type;
        }
        else
        {
            sig[2] = n2.Type;
        }

        if (p3 < 0 && p4 < 0)
        {
            var n0 = map.GetTile(x + 1, y + 1);
            sig[3] = n0 == null || n0.Desc.CompositePriority < 0 ? 255 : n0.Type;
        }
        else if (p3 < p4)
        {
            sig[3] = n4.Type;
        }
        else
        {
            sig[3] = n3.Type;
        }

        return sig;
    }

    private class CacheComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[] x, object[] y)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
        }

        public int GetHashCode(object[] obj)
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
        }
    }
    private class TileCacheComparer : IEqualityComparer<ushort>
    {
        public bool Equals(ushort x, ushort y)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
        }

        public int GetHashCode(ushort obj)
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
        }
    }
}

