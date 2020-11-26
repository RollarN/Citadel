using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpellVFXPools))]
public class SpellVFXPoolsEditor : Editor
{
    SpellVFXPools data;

    private bool m_ProjectilePrefabsFoldout = false;
    private GUIContent m_ProjectilePrefabsContent = null;
    private const string k_ProjectilePrefabsName = "m_ProjectilePrefabs";

    private bool m_ImpactPrefabsFoldout = false;
    private GUIContent m_ImpactPrefabsContent = null;
    private const string k_ImpactPrefabsName = "m_ImpactPrefabs";

    private bool m_LingeringPrefabsFoldout = false;
    private GUIContent m_LingeringPrefabsContent = null;
    private const string k_LingeringPrefabsName = "m_LingeringPrefabs";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        data = (SpellVFXPools)target;
        SetupGUI();
        data.CheckPrefabs<ElementType>();

        SerializedProperty projectileArray = data.AsSerialized.FindProperty(k_ProjectilePrefabsName);
        m_ProjectilePrefabsFoldout = EditorGUILayout.Foldout(m_ProjectilePrefabsFoldout, m_ProjectilePrefabsContent);
        if (m_ProjectilePrefabsFoldout)
        {
            for(int i = 0; i < (int)ElementType.ELEMENT_TYPE_MAX; i++)
            {
                EditorGUILayout.PropertyField(projectileArray.GetArrayElementAtIndex(i), new GUIContent(((ElementType)i).ToString()));
            }
        }

        SerializedProperty impactArray = data.AsSerialized.FindProperty(k_ImpactPrefabsName);
        m_ImpactPrefabsFoldout = EditorGUILayout.Foldout(m_ImpactPrefabsFoldout, m_ImpactPrefabsContent);
        if (m_ImpactPrefabsFoldout)
        {
            for(int i = 0; i < (int)ElementType.ELEMENT_TYPE_MAX; i++)
            {
                EditorGUILayout.PropertyField(impactArray.GetArrayElementAtIndex(i), new GUIContent(((ElementType)i).ToString()));
            }
        }

        SerializedProperty lingeringArray = data.AsSerialized.FindProperty(k_LingeringPrefabsName);
        m_LingeringPrefabsFoldout = EditorGUILayout.Foldout(m_LingeringPrefabsFoldout, m_LingeringPrefabsContent);
        if (m_LingeringPrefabsFoldout)
        {
            for(int i = 0; i < (int)ElementType.ELEMENT_TYPE_MAX; i++)
            {
                EditorGUILayout.PropertyField(lingeringArray.GetArrayElementAtIndex(i), new GUIContent(((ElementType)i).ToString()));
            }
        }

        data.AsSerialized.ApplyModifiedProperties();
    }

    private void SetupGUI()
    {
        if(m_ProjectilePrefabsContent == null)
        {
            m_ProjectilePrefabsContent = new GUIContent(nameof(data.ProjectilePrefabs));
        }
        if(m_ImpactPrefabsContent == null)
        {
            m_ImpactPrefabsContent = new GUIContent(nameof(data.ImpactPrefabs));
        }
        if(m_LingeringPrefabsContent == null)
        {
            m_LingeringPrefabsContent = new GUIContent(nameof(data.LingeringPrefabs));
        }
    }
}
