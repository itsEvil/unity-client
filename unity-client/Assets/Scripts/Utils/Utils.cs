using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static void Log(string msg) {
        Debug.Log(msg);
    }
    public static void Log(string msg, params object[] objs) {
        Debug.LogFormat(msg, objs);
    }
    public static void Warn(string msg) {
        Debug.LogWarning(msg);
    }
    public static void Warn(string msg, params object[] objs) {
        Debug.LogWarningFormat(msg, objs);
    }
    public static void Error(string msg) {
        Debug.LogError(msg);
    }
    public static void Error(string msg, params object[] objs) {
        Debug.LogErrorFormat(msg, objs);
    }
}
