using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelToggleManager : MonoBehaviour
{
    private List<EnableOnApproach> m_ToggleableLevels = new List<EnableOnApproach>();

    private void Awake()
    {
        Array.ForEach
        (
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects(),
            rootObject => Array.ForEach
                          (
                              rootObject.GetComponentsInChildren<EnableOnApproach>(true),
                              toggleableLevel => m_ToggleableLevels.Add(toggleableLevel)
                          )
        );
    }

    public void EnableAllLevels()
    {
        m_ToggleableLevels.ForEach(toggleableLevel => toggleableLevel.ToggleLevel(true));
    }
}
