using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
public static class Texture2DExtensions
{
    public static void SetColor(this Texture2D tex, Color32 color)
    {
        var fillColorArray = tex.GetPixels32();

        for(var i = 0; i < fillColorArray.Length; i++)
        {
            fillColorArray[i] = color;
        }

        tex.SetPixels32(fillColorArray);
        tex.Apply();
    }
}
#endif