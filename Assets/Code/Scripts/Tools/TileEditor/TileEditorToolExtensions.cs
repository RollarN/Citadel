using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR

public static class TileEditorToolExtensions
{
    public static int[,,] ToIntArray(this TileType[,,] tileArray)
    {



        return new int[0, 0, 0];
    }

    public static TileType[,,] ToTileTypeArray(this int[,,] intArray)
    {



        return new TileType[0, 0, 0];
    }

    public static Vector3Int ToGridPos(this Vector3 worldPos, Vector3 worldOffset, float tileSize)
    {
        return new Vector3Int(Mathf.FloorToInt(worldPos.x + Mathf.Abs(worldOffset.x)),
                                Mathf.FloorToInt(worldPos.y + Mathf.Abs(worldOffset.y)),
                                Mathf.FloorToInt(worldPos.z + Mathf.Abs(worldOffset.z)));
    }

    public static Vector3 ToWorldPos(this Vector3Int gridPos, Vector3 worldOffset, float tileSize) 
    {
        return new Vector3(gridPos.x * tileSize + worldOffset.x + tileSize * 0.5f, 
                            gridPos.y * tileSize + worldOffset.y, 
                            gridPos.z * tileSize + worldOffset.z + tileSize * 0.5f);
    }

}
#endif