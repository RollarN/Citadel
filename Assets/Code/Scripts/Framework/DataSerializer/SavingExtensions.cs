using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SavingExtensions
{
    public static string Serialize(this Vector3 v)
    {
        return $"{v.x},{v.y},{v.z}";
    }

    public static Vector3 DeserializeToVector3(this string s)
    {
        string[] splitString = s.Split(',');
        return new Vector3(float.Parse(splitString[0]), float.Parse(splitString[1]), float.Parse(splitString[2]));
    }

    public static string Serialize(this Quaternion q)
    {
        return $"{q.x},{q.y},{q.z},{q.w}";
    }
    public static Quaternion DeserializeToQuaternion(this string s)
    {
        string[] splitString = s.Split(',');
        return new Quaternion
        (
            float.Parse(splitString[0]), 
            float.Parse(splitString[1]), 
            float.Parse(splitString[2]), 
            float.Parse(splitString[3])
        );
    }
}
