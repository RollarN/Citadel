#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileEditorData))]
public class TileEditorDataEditor : Editor
{
    private const string k_PaintTypeObjectsName = "m_PaintTypeObjects";
    private GUIContent m_PaintTypeObjectsContent = null;
    private bool m_PaintTypeObjectsFoldout = false;
    TileEditorData data = null;

    public override void OnInspectorGUI()
    {
        data = (TileEditorData)target;

        SetupGUI();

        SerializedObject dataSerialized = new SerializedObject(data);

        SerializedProperty paintTypeObjects = dataSerialized.FindProperty(k_PaintTypeObjectsName);
        m_PaintTypeObjectsFoldout = EditorGUILayout.Foldout(m_PaintTypeObjectsFoldout, m_PaintTypeObjectsContent);
        if (m_PaintTypeObjectsFoldout)
        {
            for(int i = 0; i < (int)TileType.TILETYPE_MAX; i++)
            {
                //GUILayout.Label(((TileType)i).ToString());
                EditorGUILayout.PropertyField(paintTypeObjects.GetArrayElementAtIndex(i), new GUIContent(((TileType)i).ToString()));
            }
        }
        dataSerialized.ApplyModifiedProperties();
        //EditorGUILayout.PropertyField(paintTypeObjects);
    }

    private void SetupGUI()
    {
        if(m_PaintTypeObjectsContent == null)
        {
            m_PaintTypeObjectsContent = new GUIContent(nameof(data.PaintTypeObjects));
        }
    }
}
#endif