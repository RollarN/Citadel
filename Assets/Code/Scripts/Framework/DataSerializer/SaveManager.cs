using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static List<ISavable> SavableObjects = new List<ISavable>();
    private readonly List<int> m_RowIndexForObjectsInFile = new List<int>();
    private string m_SaveFilePath = string.Empty;
    private bool m_HasSavedOnce = false;

    public static event Action OnLoad;
    public static event Action OnSave;

    private void Awake()
    {
        SavableObjects.Clear();
        m_RowIndexForObjectsInFile.Clear();
        m_SaveFilePath = Application.persistentDataPath + "/SavedObjects.chkpnt";

        Array.ForEach
        (
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects(),
            rootObject => Array.ForEach
                          (
                              rootObject.GetComponentsInChildren<ISavable>(true),
                              savableObject => SavableObjects.Add(savableObject)
                          )
        );
    }

    public void SaveGame()
    {
        m_RowIndexForObjectsInFile.Clear();
        using (StreamWriter sw = new StreamWriter(m_SaveFilePath))
        {
            int lineIndex = 0;
            foreach(ISavable savableObject in SavableObjects)
            {
                m_RowIndexForObjectsInFile.Add(lineIndex);
                savableObject.ObjectActiveOnSave = ((Component)savableObject).gameObject.activeSelf;
                ((Component)savableObject).gameObject.SetActive(true);
                savableObject.OnSave(sw, out int addedCount);
                ((Component)savableObject).gameObject.SetActive(savableObject.ObjectActiveOnSave);
                lineIndex += addedCount;
            }
        }
        m_HasSavedOnce = true;

        OnSave?.Invoke();
    }

    public void LoadGame()
    {
        if (!m_HasSavedOnce)
        {
            Debug.LogWarning($"{nameof(SaveManager)} tried to save without game having been saved once.");
            return;
        }

        string[] savedData = File.ReadAllLines(m_SaveFilePath);
        for (int i = 0; i < SavableObjects.Count; i++)
        {
            SavableObjects[i].OnLoad(savedData, m_RowIndexForObjectsInFile[i]);
        }

        OnLoad?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            LoadGame();
        }
    }
}
