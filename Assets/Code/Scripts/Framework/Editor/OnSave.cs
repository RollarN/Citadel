using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class OnSave : UnityEditor.AssetModificationProcessor
{
    static string[] OnWillSaveAssets(string[] paths)
    {
        //CustomPrefabUtility.FetchAllSubsceneRootsInScene().FindAll(scene => scene != null && scene.gameObject.activeSelf).ForEach(scene => scene.CheckForPrefabOverrides());
        return paths;
    }

}
