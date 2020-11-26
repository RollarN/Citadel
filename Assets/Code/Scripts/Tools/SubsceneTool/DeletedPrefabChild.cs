
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

#if UNITY_EDITOR
public class DeletedPrefabChild : MonoBehaviour
{
    public DeletedPrefabChildrenReferences parent = null;
}
#endif