using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameManagerSpecialEditionForMainMenuShortName m_GameManagerSpecialEditionForMainMenuShortName;
    [SerializeField] private Animator m_Transition;
    [SerializeField] private float m_TransitionTime = 1f;

    public void StartGame()
    {
        StartCoroutine(FadeLevel());
    }

    private IEnumerator FadeLevel()
    {
        m_Transition.SetTrigger("Start");
        yield return new WaitForSeconds(m_TransitionTime);
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
