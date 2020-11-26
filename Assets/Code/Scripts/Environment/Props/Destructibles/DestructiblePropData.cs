using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DestructiblePropData", menuName = "ScriptableObjects/DestructiblePropData")]
public class DestructiblePropData : ScriptableObject
{
    public LayerMask PartsMask;
}
