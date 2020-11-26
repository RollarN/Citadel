using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyPropExtensions
{

    /// <summary>
    /// Returns the same Vector3 where Y = 0
    /// </summary>
    public static Vector3 Flattened(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

}
