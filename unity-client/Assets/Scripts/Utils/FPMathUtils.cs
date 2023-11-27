using System.Collections.Generic;
using UnityEngine;
public static class FPMathUtils
{
    private static float[] _floatBuffer;
    private static ushort[] _ushortBuffer;
    private static byte _floatIndex;
    private static byte _ushortIndex;
    private const byte _size = 100;
    static FPMathUtils() => Init();
    public static void Init()
    {
        _ushortBuffer = new ushort[_size];
        _floatBuffer = new float[_size];

        for (int i = 0; i < _size; i++)
        {
            _ushortBuffer[i] = (ushort)Random(ushort.MaxValue, ushort.MaxValue);
            _floatBuffer[i] = Random(float.MinValue, float.MaxValue);
        }
    }
    public static float BoundToPI(float x)
    {
        int v;
        if (x < -Mathf.PI)
        {
            v = ((int)(x / -Mathf.PI) + 1) / 2;
            x += v * 2 * Mathf.PI;
        }
        else if (x > Mathf.PI)
        {
            v = ((int)(x / Mathf.PI) + 1) / 2;
            x -= v * 2 * Mathf.PI;
        }
        return x;
    }

    public static float Distance(float x, float y, float x1, float y1)
    {
        var dx = x - x1;
        var dy = y - y1;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public static float DistanceSqr(float x, float y, float x1, float y1)
    {
        var dx = x - x1;
        var dy = y - y1;
        return dx * dx + dy * dy;
    }

    public static float DistanceSquared(Vector3 from, Vector3 to)
    {
        float v1 = from.x - to.x, v2 = from.y - to.y;
        return v1 * v1 + v2 * v2;
    }

    public static T Random<T>(this List<T> list)
    {
        return list[Random(0, list.Count)];
    }
    /// <returns>Returns a value between min and (max - 1)</returns>
    public static int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    public static bool Chance(float chance)
    {
        return UnityEngine.Random.Range(0f, 1f) <= chance;
    }
    public static float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    /// <returns> True if next value in _floatBuffer is less than or equal to chance</returns>
    public static bool ChanceFast(float chance)
    {
        return RandomFloat() <= chance;
    }
    /// <returns> Random value between float.MinValue and float.MaxValue</returns>
    public static float RandomFloat()
    {
        if (_floatIndex + 1 == _floatBuffer.Length)
            _floatIndex = 0;

        return _floatBuffer[_floatIndex++];
    }
    /// <returns> Random value between ushort.MinValue and ushort.MaxValue</returns>
    public static ushort RandomUshort()
    {
        if (_ushortIndex + 1 == _ushortBuffer.Length)
            _ushortIndex = 0;

        return _ushortBuffer[_ushortIndex++];
    }

    public static byte RandomRef(ref byte[] bytes, ref byte index)
    {
        if (index + 1 == bytes.Length)
            index = 0;

        return bytes[index++];
    }

    public static T[] ToArrayFill<T>(this T[] _, T fillValue, int length)
    {
        T[] array = new T[length];

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = fillValue;
        }
        return array;
    }

    public static T[] FillArray<T>(this T[] array, T fillValue)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = fillValue;
        }
        return array;
    }
}

