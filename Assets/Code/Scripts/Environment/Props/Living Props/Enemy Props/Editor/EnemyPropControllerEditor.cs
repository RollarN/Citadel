#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyPropController))]
public class EnemyPropControllerEditor : Editor
{
    private EnemyPropController m_Target = null;
    private const string k_ChestData = "Chest Data";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        m_Target = (EnemyPropController)target;

        if(m_Target.AttackPattern == MimicAttackPattern.ChestJump)
        {
            GUILayout.Label(k_ChestData);

        }
    }
}
#endif