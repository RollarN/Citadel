
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.VersionControl;
using System.Diagnostics.Eventing.Reader;

public class CustomPrefabUtility
{
    public static void FindPrefabInstanceRootAndApplyChanges(GameObject prefabPart, InteractionMode interactionMode)
    {
        GameObject prefabInstanceRoot = FindNearestPrefabInstance(prefabPart);
        if (prefabInstanceRoot != null)
        {
            PrefabUtility.ApplyPrefabInstance(prefabInstanceRoot, InteractionMode.UserAction);
        }
        else
        {
            Debug.LogWarning("GameObject is not part of any PrefabInstance.");
        }
    }

    public static bool HasNearestPrefabInstanceRootAnyOverrides(GameObject prefabPart)
    {
        GameObject prefabInstanceRoot = FindNearestPrefabInstance(prefabPart);

        foreach(Transform child in prefabInstanceRoot.transform)
        {
            if (CheckForOverridesInChildren(child))
            {
                return true;
            }
        }
        return false;
    }

    private static bool CheckForOverridesInChildren(Transform transform)
    {
        if(PrefabUtility.IsAddedGameObjectOverride(transform.gameObject))
        {
            return true;
        }

        foreach (Transform child in transform)
        {
            if (CheckForOverridesInChildren(child))
            {
                return true;
            }    
        }
        return false;
    }

    public static GameObject FindNearestPrefabInstance(GameObject prefabPart)
    {
        Transform partTransform = prefabPart.transform;
        while (partTransform != null)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(partTransform.gameObject))
            {
                break;
            }
            partTransform = partTransform.parent;
        }
        if(partTransform == null)
        {
            Debug.LogWarning("GameObject is not part of any Prefab Instance: " + prefabPart.name);
            return null;
        }
        else
        {
            return partTransform.gameObject;
        }
        
    }

    public static List<SubsceneRoot> FetchAllSubsceneRootsInScene()
    {
        GameObject sceneRoot = GameObject.Find(SubsceneToolData.SceneRootName);
        if (sceneRoot)
        {
            return new List<SubsceneRoot>(sceneRoot.GetComponentsInChildren<SubsceneRoot>(true));
        }
        else
        {
            return new List<SubsceneRoot>();
        }
    }

    public static bool CheckOutPrefabInstanceIfValid(GameObject prefabPart, string pathToAsset)
    {
        Task statusTask = Provider.Status(pathToAsset);
        statusTask.Wait();
        if (IsPrefabCheckedoutLocal(ref statusTask))
        {
            return true;
        }
        else if (IsPrefabCheckedOutRemote(statusTask))
        {
            return false;
        }
        else if (Provider.CheckoutIsValid(statusTask.assetList[0]))
        {
            Task coTask = Provider.Checkout(statusTask.assetList[0], CheckoutMode.Both);
            coTask.Wait();
            return true;
        }
        return false;
    }

    public static bool IsPrefabCheckedOutRemote(Task statusTask)
    {
        if (statusTask.assetList[0].IsState(Asset.States.CheckedOutRemote) || statusTask.assetList[0].IsState(Asset.States.LockedRemote))
        {
            return true;
        }
        return false;
    }

    public static bool IsPrefabCheckedoutLocal(ref Task statusTask) 
    {
        if(statusTask.assetList[0].IsState(Asset.States.CheckedOutLocal) || statusTask.assetList[0].IsState(Asset.States.LockedLocal))
        {
            return true;
        }
        return false;
    }
}
#endif