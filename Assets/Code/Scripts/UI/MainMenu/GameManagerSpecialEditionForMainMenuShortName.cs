using UnityEngine;

public class GameManagerSpecialEditionForMainMenuShortName : MonoBehaviour
{
    [SerializeField] private string m_GameSceneName;
    public string GameSceneName => m_GameSceneName;

    private void Awake()
    {
        if (m_GameSceneName.Length < 1)
        {
            m_GameSceneName = "Main";
        }
    }
}
