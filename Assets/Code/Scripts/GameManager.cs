using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SpellVFXPools m_ProjectileVFXPools = null;
    private static int s_MaxAmountOfPlayerObjectsInScene = 1;
    public static int MaxAmountOfPlayerObjectsInScene
    {
        get => s_MaxAmountOfPlayerObjectsInScene;
    }

    [SerializeField] private string m_GameSceneName;
    [SerializeField] private string m_MainMenuSceneName;
    [SerializeField] private GameObject m_Player = null;

    public string GameSceneName => m_GameSceneName;
    public string MainMenuSceneName => m_MainMenuSceneName;
    public GameObject Player => m_Player;

    private void Awake()
    {
        if (m_ProjectileVFXPools)
        {
            m_ProjectileVFXPools.GeneratePools();
        }
    }

    void Update()
    {
        //Debug.Log(PrefabUtility.GetPrefabInstanceHandle(Selection.activeGameObject) != null);
    }
}