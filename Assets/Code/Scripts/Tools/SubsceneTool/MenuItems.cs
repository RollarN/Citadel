#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class MenuItems
{
    private static GameObject s_DeletedPrefabChildsParent = null;
    private readonly static Stack<int> s_DeleteBatchSize = new Stack<int>();
    private readonly static Stack<GameObject> s_DeletedObjects = new Stack<GameObject>();
    private const string k_DeletedPrefabChildsParentName = "DeletedPrefabChildren";
    public static GameObject DeletedPrefabChildsParent
    {
        get 
        { 
            if (s_DeletedPrefabChildsParent == null)
            {
                GameObject parent = GameObject.Find(k_DeletedPrefabChildsParentName);
                if (!parent)
                {
                    s_DeletedPrefabChildsParent = new GameObject(k_DeletedPrefabChildsParentName);
                }
                else
                {
                    s_DeletedPrefabChildsParent = parent;
                }
            }
            return s_DeletedPrefabChildsParent;
        }
        set
        {
            s_DeletedPrefabChildsParent = value;
        }
    }


    [MenuItem("GameObject/SubSceneTools/Delete Selected Prefab Children", priority = -100)]
    static void DeletePrefabChildren()
    {
        Stack<GameObject> outerMostObjects = new Stack<GameObject>();
        Stack<GameObject> objectsToDeleteSorted = new Stack<GameObject>();
        foreach(GameObject go in Selection.gameObjects)
        {
            Transform parent = go.transform.parent;
            while(parent != null)
            {
                if (Selection.gameObjects.Contains<GameObject>(parent.gameObject))
                {
                    break;
                }
                parent = parent.parent;
            }
            if(parent == null)
            {
                outerMostObjects.Push(go);
            }
        }
        //foreach(GameObject go in outerMostObjects)
        //{
        //    AddSelectedObjectsChildrenFirst(objectsToDeleteSorted, go.transform);
        //}
        s_DeleteBatchSize.Push(outerMostObjects.Count);

        while(outerMostObjects.Count > 0)
        {
            GameObject go = outerMostObjects.Pop();
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                GameObject goCopy = Object.Instantiate(go, DeletedPrefabChildsParent.transform);
                goCopy.name = go.name;
                goCopy.SetActive(false);

                
                DeletedPrefabChild goCopyData = goCopy.AddComponent<DeletedPrefabChild>();
                DeletedPrefabChildrenReferences goChildReference = go.transform.parent.gameObject.GetComponent<DeletedPrefabChildrenReferences>();
                if (goChildReference == null)
                {
                    goChildReference = go.transform.parent.gameObject.AddComponent<DeletedPrefabChildrenReferences>();
                }
                goChildReference.deletedChildren.Add(goCopyData);
                goCopyData.parent = goChildReference;


                s_DeletedObjects.Push(goCopy);

                Debug.Log("Deleting items");

                if (PrefabUtility.IsOutermostPrefabInstanceRoot(go) || go.transform.parent == null)
                {
                    GameObject.DestroyImmediate(go);
                    continue;
                }


                Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(go);
                GameObject prefab = PrefabUtility.GetNearestPrefabInstanceRoot(go.transform.parent);
                Object.DestroyImmediate(prefabInstance);
                Object.DestroyImmediate(go);
                Debug.Log("Deleted");
                PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.AutomatedAction);
            }
            else
            {
                Debug.Log("Not part of a prefab instance: " + go.name);
            }
        }
        
    }

    [MenuItem("GameObject/SubSceneTools/Delete Selected Prefab Children", true)]
    static bool ValidateDeletePrefabChildren()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Can't delete prefab children in play mode");
            return false;
        }
        foreach(GameObject go in Selection.gameObjects)
        {
            if(PrefabUtility.GetNearestPrefabInstanceRoot(go) != null)
            {
                return true;
            }
        }
        Debug.Log("No selected GameObject is part of a prefab instance. Please use the default Delete.");
        return false;
    }

    private static void AddSelectedObjectsChildrenFirst(Stack<GameObject> sortedObjects, Transform child)
    {
        foreach (Transform grandchild in child)
        {
            AddSelectedObjectsChildrenFirst(sortedObjects, grandchild);
        }
        sortedObjects.Push(child.gameObject);
    }


    [MenuItem("GameObject/SubSceneTools/Undo Deletion of last prefab child", priority = -99)]
    static void UndoDeletion()
    {
        if(s_DeletedObjects.Count <= 0 || s_DeleteBatchSize.Count <= 0)
        {
            s_DeletedObjects.Clear();
            s_DeleteBatchSize.Clear();

            if(DeletedPrefabChildsParent.transform.childCount > 0)
            {
                Debug.Log("Batches were unserialized. Undoing one at a time. (Prefab Function / Undo)");
                foreach(Transform child in DeletedPrefabChildsParent.transform)
                {
                    s_DeletedObjects.Push(child.gameObject);
                    s_DeleteBatchSize.Push(1);
                }
            }
            else
            {
                Debug.Log("Nothing to undo.");
                return;
            }
        }
        int undos = s_DeleteBatchSize.Pop();
        for(int i = 0; i < undos; i++)
        {
            if (CheckIfParentExist())
            {
                GameObject goCopy = s_DeletedObjects.Pop();
                DeletedPrefabChildrenReferences parentReference = goCopy.GetComponent<DeletedPrefabChild>().parent;
                

                goCopy.transform.parent = parentReference.transform;
                goCopy.SetActive(true);
                DeletedPrefabChild prefabChildReference = goCopy.GetComponent<DeletedPrefabChild>();
                parentReference.deletedChildren.Remove(prefabChildReference);
                GameObject.DestroyImmediate(prefabChildReference);
                CustomPrefabUtility.FindPrefabInstanceRootAndApplyChanges(goCopy, InteractionMode.UserAction);
            }
        }
    }


    [MenuItem("GameObject/SubSceneTools/Undo Deletion of last prefab child", true)]
    static bool ValidateUndoDeletion()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Can't undo in play mode");
        }

        if(s_DeletedObjects.Count <= 0)
        {
            if(DeletedPrefabChildsParent.transform.childCount > 0)
            {
                foreach(Transform child in DeletedPrefabChildsParent.transform)
                {
                    s_DeletedObjects.Push(child.gameObject);
                }
            }
        }

        return true;
    }

    static bool CheckIfParentExist()
    {
        if (s_DeletedObjects.Peek().GetComponent<DeletedPrefabChild>().parent == null)
        {
            Debug.Log("Parent no longer exist");
            if (s_DeletedObjects.Peek() != null)
            {
                Object.DestroyImmediate(s_DeletedObjects.Peek());
            }
            s_DeletedObjects.Pop();
            return false;
        }
        return true;
    }

    [MenuItem("GameObject/TileTool/MergeChildrenMeshIntoParent", priority = -100)]
    static void MergeChildrenIntoParent()
    {
        if(PrefabUtility.GetPrefabAssetType(Selection.activeGameObject) != PrefabAssetType.NotAPrefab)
        {
            Debug.LogError("(Merge meshes) Can't merge existing prefabs. Unpack prefab and turn it into a new one if need be.");
            return;
        }
        else if(Selection.gameObjects.Length > 1)
        {
            Debug.LogError("(Merge meshes) Multiple objects isn't supported.");
            return;
        }
        else if (Application.isPlaying)
        {
            Debug.LogError("(Merge meshes) Cannot merge meshes in PlayMode");
            return;
        }

        Vector3 oldPosition = Selection.activeGameObject.transform.position;
        Quaternion oldRotation = Selection.activeGameObject.transform.rotation;
        Selection.activeGameObject.transform.position = Vector3.zero;
        Selection.activeGameObject.transform.rotation = Quaternion.identity;

        MeshFilter parentFilter;

        if(!Selection.activeGameObject.TryGetComponent(out parentFilter))
        {
            parentFilter = Selection.activeGameObject.AddComponent<MeshFilter>();
        }
        MeshRenderer parentRenderer;
        if(!Selection.activeGameObject.TryGetComponent(out parentRenderer))
        {
            parentRenderer = Selection.activeGameObject.AddComponent<MeshRenderer>();
        }

        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        for(int i = 0; i < meshFilters.Length - 1; i++)
        {
            combine[i].mesh = meshFilters[i + 1].sharedMesh;
            combine[i].transform = meshFilters[i + 1].transform.localToWorldMatrix;
        }

        parentFilter.mesh = new Mesh();
        parentFilter.sharedMesh.CombineMeshes(combine);
         
        for(int i = 1; i < meshFilters.Length; i++)
        {
            if(meshFilters[i] != null)
            {
                GameObject.DestroyImmediate(meshFilters[i].gameObject);
            }
        }

        Selection.activeGameObject.transform.position = oldPosition;
        Selection.activeGameObject.transform.rotation = oldRotation;
    }
}
#endif