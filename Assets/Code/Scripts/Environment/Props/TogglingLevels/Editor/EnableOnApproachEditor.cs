#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnableOnApproach))]
public class EnableOnApproachEditor : Editor
{
    private const string m_ToggleLevel = "Toggle Level";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnableOnApproach eoa = target as EnableOnApproach;

        if (GUILayout.Button(m_ToggleLevel))
        {
            eoa.ToggleLevel(!eoa.LevelEnabled);
        }
    }

    private void OnSceneGUI()
    {
        EnableOnApproach eoa = target as EnableOnApproach;

        Handles.DrawWireDisc(eoa.transform.position + eoa.DetectionOffset, Vector3.up, eoa.DetectionRadius);
        //Handles.SphereHandleCap(
        //    0,
        //    eoa.transform.position + eoa.DetectionOffset,
        //    Quaternion.identity,
        //    eoa.DetectionRadius,
        //    EventType.Repaint
        //);
    }
}
#endif