using System;
using System.Text;
using System.Security.Cryptography;
public static class EncryptionUtils
{
    private static SHA256Managed _sHA256Managed = new SHA256Managed();
    public static string ToSHA256(string val) => Convert.ToBase64String(_sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(val)));
}

