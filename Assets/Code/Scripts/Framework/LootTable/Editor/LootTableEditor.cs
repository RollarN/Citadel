#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WeightedLootTable))]
public class LootTableEditor : Editor
{
    private WeightedLootTable m_Table;
    private SerializedProperty m_ItemNames;
    private SerializedProperty m_Probability;
    private EnumToolData m_EnumToolData = null;

    private void OnEnable()
    {
        Array.ForEach(AssetDatabase.FindAssets("t:EnumToolData"), 
            asset => m_EnumToolData = AssetDatabase.LoadAssetAtPath<EnumToolData>(AssetDatabase.GUIDToAssetPath(asset)));

        m_EnumToolData.OnChange += UpdateItemProperties;
    }

    public override void OnInspectorGUI()
    {
        m_Table = (WeightedLootTable)target;

        UpdateArrays();

        ApplyModifiedProperties();
    }

    private void UpdateArrays()
    {
        // because magic
        if (m_Table.Trackables.Length != (int)ItemEnums.ITEM_ENUMS_MAX)
        {
            UpdateItemProperties();
        }

        if (m_Table.Trackables.Length == (int)ItemEnums.ITEM_ENUMS_MAX)
        {
            ItemProperty();
        }
    }

    private void ItemProperty()
    {
        m_ItemNames = serializedObject.FindProperty("m_ItemNames");
        EditorGUILayout.PropertyField(m_ItemNames);
        EditorHelperFunctions.Indent();

        for (int i = 0; i < (int)ItemEnums.ITEM_ENUMS_MAX; i++)
        {
            if (m_ItemNames == null)
                return;

            EditorGUILayout.PropertyField(m_ItemNames.GetArrayElementAtIndex(i), new GUIContent(((ItemEnums)i).ToString()));
        }

        EditorHelperFunctions.Indent(-1);
        m_Probability = serializedObject.FindProperty("m_ProbabilityToDrop");
        EditorGUILayout.PropertyField(m_Probability);
        EditorHelperFunctions.Indent();

        for (int i = 0; i < (int)ItemEnums.ITEM_ENUMS_MAX; i++)
        {
            EditorGUILayout.PropertyField(m_Probability.GetArrayElementAtIndex(i), new GUIContent(((ItemEnums)i).ToString()));
        }
    }

    private void UpdateItemProperties()
    {
        if (m_Table.Trackables.Length != (int)ItemEnums.ITEM_ENUMS_MAX)
        {
            m_Table.Trackables = new ItemEnums[(int)ItemEnums.ITEM_ENUMS_MAX];
            ResizeItemLists((int)ItemEnums.ITEM_ENUMS_MAX);

            for (int i = 0; i < (int)ItemEnums.ITEM_ENUMS_MAX; i++)
            {
                m_Table.Trackables[i] = (ItemEnums)i;
            }
        }

        if (m_ItemNames == null)
            return;

        ItemProperty();
    }

    private void ResizeItemLists(int currentLength)
    {
        if (m_Table.ItemNames.Length != currentLength)
        {
            Array.Resize(ref m_Table.m_ItemNames, currentLength);
            Array.Resize(ref m_Table.m_ProbabilityToDrop, currentLength);
        }
    }

    private void ApplyModifiedProperties()
    {
        serializedObject.ApplyModifiedProperties();
    }
}
#endif