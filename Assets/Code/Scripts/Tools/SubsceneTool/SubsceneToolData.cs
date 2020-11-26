#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CreateAssetMenu(fileName="SubsceneToolData", menuName = "ScriptableObjects/SubsceneToolData")]
public class SubsceneToolData : ScriptableObject
{
    [NonSerialized] private List<SubsceneRoot> m_Subscenes = new List<SubsceneRoot>();
    [NonSerialized] public bool[] SubscenesActiveState = new bool[0];

    public const string SceneRootName = "SceneRoot";

    public SerializedObject AsSerialized; 

    public List<SubsceneRoot> Subscenes 
    {
        get
        {
            if(m_Subscenes.Count == 0)
            {
                m_Subscenes = CustomPrefabUtility.FetchAllSubsceneRootsInScene();
                if(m_Subscenes == null)
                {
                    m_Subscenes = new List<SubsceneRoot>();
                }
            }
            return m_Subscenes;
        }
        set
        {
            m_Subscenes = value;
        }
    }

    private void OnEnable()
    {
        AsSerialized = new SerializedObject(this);
    }

    public bool CheckNewOrRemovedSubscenes()
    {
        //UnityEngine.Object.FindObjectsOfType(typeof(SubsceneRoot), true);
        //SubsceneRoot[] subscenesToCheck = Resources.FindObjectsOfTypeAll<SubsceneRoot>();

        //Debug.Log(subscenesToCheck.Length);
        //if(subscenesToCheck.Length != Subscenes.Length)
        //{
        //    UpdateArrays(subscenesToCheck);
        //    return true;
        //}
        
        //for(int i = 0; i < Subscenes.Length; i++)
        //{
        //    if(Subscenes[i] != subscenesToCheck[i])
        //    {
        //        UpdateArrays(subscenesToCheck);
        //        return true;
        //    }
        //}
        return false;
    }

    public void UpdateActiveStates()
    {
        Subscenes.RemoveAll(scene => scene == null);
        SubscenesActiveState = new bool[Subscenes.Count];

        for(int i = 0; i < SubscenesActiveState.Length; i++)
        {
            SubscenesActiveState[i] = Subscenes[i].gameObject.activeSelf;
        }
    }

    public void UpdateSubsceneStates()
    {
        for(int i = 0; i < Subscenes.Count; i++)
        {
            bool saveScene = Subscenes[i].gameObject.activeSelf != SubscenesActiveState[i];
            Subscenes[i].gameObject.SetActive(SubscenesActiveState[i]);
            if (saveScene)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }

    private void UpdateArrays(SubsceneRoot[] subscenesToCheck)
    {
        //SubsceneRoot[] newSceneArray = new SubsceneRoot[subscenesToCheck.Length];
        //bool[] newActiveArray = new bool[subscenesToCheck.Length];

        //for(int i = 0; i < subscenesToCheck.Length; i++)
        //{
        //    bool alreadyExists = false;
        //    for(int j = 0; j < Subscenes.Length; j++)
        //    {
        //        if(Subscenes[j] == subscenesToCheck[i])
        //        {
        //            newSceneArray[i] = Subscenes[j];
        //            newActiveArray[i] = ActiveSubscenes[j];
        //            newActiveArray[i] = newSceneArray[i].gameObject.activeSelf;
        //            alreadyExists = true;
        //            break;
        //        }
        //    }

        //    if (!alreadyExists)
        //    {
        //        newSceneArray[i] = subscenesToCheck[i];
        //        newActiveArray[i] = newSceneArray[i].gameObject.activeSelf;
        //    }
        //}

        //Subscenes = newSceneArray;
        //ActiveSubscenes = newActiveArray;
    }
}
#endif