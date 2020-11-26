#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
[CustomEditor(typeof(TileEditorTool))]
public class TileEditorToolEditor : Editor
{
    TileEditorTool tool;
    void OnSceneGUI()
    {

    }
}
#endif