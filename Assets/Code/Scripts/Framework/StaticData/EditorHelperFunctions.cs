using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public static class EditorHelperFunctions
{
	// Test comment written in Anteckningar, Anteckningar > Visual Studio. Doggos

    private static GUIContent ToolTip(string name, string toolTip)
    {
        return new GUIContent(name, toolTip);
    }

    public static void WhiteSpace()
    {
        EditorGUILayout.Space();
    }

    public static void Indent(int level = 1)
    {
        EditorGUI.indentLevel += level;
    }

    public static GameObject GameObjectField(GameObject gameObject, string name, string toolTip, bool changeAble = true)
    {
        return gameObject = (GameObject)EditorGUILayout.ObjectField(ToolTip(name, toolTip), gameObject, typeof(GameObject), changeAble);
    }

    public static Transform TransformField(Transform transform, string name, string toolTip, bool changeAble = true)
    {
        return transform = (Transform)EditorGUILayout.ObjectField(ToolTip(name, toolTip), transform, typeof(Transform), changeAble);
    }

    public static int IntField(int i, string name, string toolTip)
    {
        return i = EditorGUILayout.IntField(ToolTip(name, toolTip), i);
    }

    public static float FloatField(float f, string name, string toolTip)
    {
        return f = EditorGUILayout.FloatField(ToolTip(name, toolTip), f);
    }
}
#endif