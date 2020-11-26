#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DeletedPrefabChildrenReferences : MonoBehaviour
{
    public List<DeletedPrefabChild> deletedChildren = new List<DeletedPrefabChild>();

    private void OnEnable()
    {
        foreach (DeletedPrefabChild child in deletedChildren)
        {
            if (child)
            {
                child.parent = this;
            }
        }

        InvokeRepeating("DestroyIfEmpty", 5, 30);
    }

    private void DestroyIfEmpty()
    {
        deletedChildren.RemoveAll(item => item == null);

        if (deletedChildren.Count <= 0)
        {
            DestroyImmediate(this);
            return;
        }
    }
}

#endif