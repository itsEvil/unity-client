﻿using System.Text;
public static class StringUtils
{
    private const string _errorOne = "<Error>";
    private const string _errorTwo = "</Error>";
    private const string _colorOne = "<color=0>";
    private const string _colorTwo = "</color>";
    /// <summary>
    /// Adds a XML <Error> tag around the msg param
    /// </summary>
    public static string AddErrorTag(string msg)
    {
        StringBuilder sb = new();
        sb.Append(_errorOne);
        sb.Append(msg);
        sb.Append(_errorTwo);
        return sb.ToString();
    }
    public static string AddErrorTag(object obj)
    {
        StringBuilder sb = new();
        sb.Append(_errorOne);
        sb.Append(obj);
        sb.Append(_errorTwo);
        return sb.ToString();
    }
    /// <summary>
    /// Adds a XML <color> tag with the specified hex color value around the msg param
    /// </summary>
    public static string AddColorTag(string msg, string color)
    {
        StringBuilder sb = new();
        sb.Append(_colorOne).Replace("0", color);
        sb.Append(msg);
        sb.Append(_colorTwo);
        return sb.ToString();
    }
}

