using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIData", menuName = "ScriptableObjects/AIData")]
public class AIData : ScriptableObject
{
    public LayerMask playerMask;
}
