using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameManager m_GameManager;
    [SerializeField] private AudioMixer m_VolumeMixer;

    [Header("UI")]
    [SerializeField] private GameObject m_PauseMenuUI;
    [SerializeField] private GameObject m_SettingsMenuUI;
    [SerializeField] private GameObject m_ControlMenuUI;

    public static bool GameIsPaused { get; private set; }

    private void DisableUI()
    {
        m_PauseMenuUI.SetActive(false);
        m_SettingsMenuUI.SetActive(false);
    }

    private void Awake()
    {
        GameIsPaused = false;
        DisableUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_ControlMenuUI.activeInHierarchy)
            {
                ControlsBack();
                return;
            }

            if (m_SettingsMenuUI.activeInHierarchy)
            {
                SettingsBack();
                return;
            }

            if (GameIsPaused)
            {
                Resume();
            }
            else { Pause(); }
        }
    }

    private void Pause()
    {
        m_PauseMenuUI.SetActive(true);
        GameIsPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        m_PauseMenuUI.SetActive(false);
        GameIsPaused = false;
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        if (m_GameManager.MainMenuSceneName.Length < 1)
        {
            Debug.LogError("Main Menu scene name is invalid. Check GameManager");
            return;
        }

        SceneManager.LoadScene(m_GameManager.MainMenuSceneName);
    }

    public void Controls()
    {
        m_ControlMenuUI.SetActive(true);
        m_PauseMenuUI.SetActive(false);
    }

    public void ControlsBack()
    {
        m_ControlMenuUI.SetActive(false);
        m_PauseMenuUI.SetActive(true);
    }

    public void Settings()
    {
        m_SettingsMenuUI.SetActive(true);
        m_PauseMenuUI.SetActive(false);
    }

    public void SettingsBack()
    {
        m_SettingsMenuUI.SetActive(false);
        m_PauseMenuUI.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(m_GameManager.GameSceneName);

        if (GameIsPaused)
        {
            DisableUI();
            Resume();
        }
    }

    public void SetMusicVolume(float volume)
    {
        m_VolumeMixer.SetFloat("MusicVolume", volume);
    }

    public void SetSoundVolume(float volume)
    {
        m_VolumeMixer.SetFloat("SoundVolume", volume);
    }

    public void SetMasterVolume(float volume)
    {
        m_VolumeMixer.SetFloat("MasterVolume", volume);
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