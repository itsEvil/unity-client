using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    //Addresses
    public const string API_ADDRESS = "http://127.0.0.1:8080";
    public const string GAME_ADDRESS = "127.0.0.1";
    public const int GAME_PORT = 2050;

    //Game Version
    public const ushort MajorBuild = 0;
    public const ushort MinorBuild = 0;

    //World Ids
    public const int NexusId = -1;

    public static float CameraAngle; // in radians
}

