using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DialogStyle
{
    World,
    Screen
}

public class DialogManager : MonoBehaviour
{
    // Hierarchy references
    [SerializeField] private GameObject m_DialogRoot = null;
    [SerializeField] private Image m_MainDialogImage = null;
    [SerializeField] private Image m_MainDialogTextBackgroundImage = null;
    [SerializeField] private Text m_MainDialogTitleText = null;
    [SerializeField] private Text m_MainDialogSubTitleText = null;
    [SerializeField] private Text m_MainDialogContentText = null;

    // Limit
    [SerializeField] private int m_MaxAmountOfDialogs = 100;
    [SerializeField] private float m_LettersPerSecond = 20f;
    [SerializeField] private float m_PrintFinishedBuffer = 1f;


    private MainDialogData m_CurrentMainDialog = null;

    private int m_WorldDialogCount = 0;
    private bool m_PrintingText = false;
    private float m_TimeAtLastLetter = 0f;
    private readonly HashSet<int> m_WorldDialogIDs = new HashSet<int>();
    private readonly Dictionary<int, WorldDialogData> m_WorldDialogs = new Dictionary<int, WorldDialogData>();
    private readonly Queue<MainDialogData> m_QueuedMainDialogs = new Queue<MainDialogData>();
    private IEnumerator m_TextPrinter = null;

    public event Action OnDialogDone;
    public int WorldDialogCount => m_WorldDialogCount;
    public MainDialogData CurrentMainDialog => m_CurrentMainDialog;
    public GameObject DialogRoot => m_DialogRoot;
    public Image MainDialogImage => m_MainDialogImage;
    public Text MainDialogTitleText => m_MainDialogTitleText;
    public Text MainDialogSubtitleText => m_MainDialogSubTitleText;
    public Text MainDialogContentText => m_MainDialogContentText;

    private void Awake()
    {
        m_TextPrinter = PrintText();
    }

    private void Update()
    {
        UpdateMainDialog();

        foreach (WorldDialogData dialogData in m_WorldDialogs.Values)
        {
            UpdateWorldDialog(dialogData);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            GoToNextDialogIfPossible();
        }
    }


    /// <summary>
    /// Displays a dialog on already defined areas.
    /// </summary>
    /// <param name="image">Associated image (e.g Character)</param>
    /// <param name="title">Title to display (e.g Name, Origin...).</param>
    /// <param name="subtitle">Subtitle to display (e.g Character title, Origin property).</param>
    /// <param name="text">Text to display.</param>
    /// <param name="displayTime">How long the dialog should stay. 0 = forever.</param>
    /// <param name="overrideCurrentDialog">Overrides the current dialog and removes all queued dialogs.</param>
    public void SendMainDialog(Sprite image, string title, string subtitle, string text, float displayTime, bool overrideCurrentDialog = false)
    {
        if(overrideCurrentDialog) 
        {
            m_QueuedMainDialogs.Clear();
            m_CurrentMainDialog.Hide();
            m_CurrentMainDialog = new MainDialogData(this, m_DialogRoot, image, title, subtitle, text, displayTime);
            DisplayMainDialog(m_CurrentMainDialog);
            BeginPrintingText();
        }
        else if(CurrentMainDialog != null && m_PrintingText)
        {
            m_QueuedMainDialogs.Enqueue(new MainDialogData(this, m_DialogRoot, image, title, subtitle, text, displayTime));
        }
        else
        {
            Debug.Log("Setting maindialog");
            m_CurrentMainDialog = new MainDialogData(this, m_DialogRoot, image, title, subtitle, text, displayTime);
            DisplayMainDialog(m_CurrentMainDialog);
            BeginPrintingText();
        }
    }

    /// <summary>
    /// Displays a dialog in world space.
    /// </summary>
    /// <param name="parent">Object to follow in the world.</param>
    /// <param name="offset">Offset in relation to object.</param>
    /// <param name="dialogStyle">Should the dialog be presented flat towards the screen or have its own world rotation?</param>
    /// <param name="title">Title to display (e.g Name, Origin...).</param>
    /// <param name="text">Subtitle to display (e.g Character title, Origin property).</param>
    /// <returns>Returns its ID.</returns>
    public int SendWorldDialog(Transform parent, Vector3 offset, DialogStyle dialogStyle, string title, string text)
    {
        if (!IsWorldDialogSendable())
        {
            return -1;
        }

        int id = FetchID();
        WorldDialogData dialogData = new WorldDialogData(parent, offset, dialogStyle, title, text, id);

        m_WorldDialogIDs.Add(id);
        m_WorldDialogs.Add(id, dialogData);
        m_WorldDialogCount++;

        switch (dialogStyle)
        {
            case DialogStyle.Screen:
                Spawn2DDialog(dialogData);
                break;
            case DialogStyle.World:
                Spawn3DDialog(dialogData);
                break;
        }


        return id;
    }

    public WorldDialogData GetWorldDialog(int id)
    {
        if (m_WorldDialogIDs.Contains(id))
        {
            return m_WorldDialogs[id];
        }

        Debug.LogError($"Could not find World Dialog. ID: {id}.");
        return null;
    }

    private void Spawn2DDialog(WorldDialogData dialogData)
    {
        Debug.LogWarning($"{nameof(Spawn2DDialog)} not implemented.");
    }

    private void Spawn3DDialog(WorldDialogData dialogData)
    {
        Debug.LogWarning($"{nameof(Spawn3DDialog)} not implemented.");
    }

    private void DisplayMainDialog(MainDialogData dialogData)
    {
        dialogData.Display();
        m_MainDialogTitleText.rectTransform.sizeDelta = new Vector2(GenerateUIWidthOfText(m_MainDialogTitleText), m_MainDialogTitleText.rectTransform.sizeDelta.y);
    }

    private float GenerateUIWidthOfText(Text text)
    {
        float length = 0f;
        foreach(char ch in text.text)
        {
            text.font.GetCharacterInfo(ch, out CharacterInfo info, text.fontSize);
            length += info.advance;
        }
        return length;
    }

    private void HideMainDialog(MainDialogData dialogData)
    {
        dialogData.Hide();
    }

    private IEnumerator PrintText()
    {
        m_PrintingText = true;
        m_MainDialogContentText.text = string.Empty;
        m_TimeAtLastLetter = Time.time;
       
        for (int i = 0; m_CurrentMainDialog != null && i < m_CurrentMainDialog.Content.Length; i++)
        {
            m_MainDialogContentText.text += m_CurrentMainDialog.Content[i];
            m_TimeAtLastLetter += 1f / m_LettersPerSecond;
            if (m_TimeAtLastLetter < Time.time)
            {
                continue;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(m_PrintFinishedBuffer);
        m_PrintingText = false;
    }

    private bool IsWorldDialogSendable()
    {
        if (m_WorldDialogCount == m_MaxAmountOfDialogs)
        {
            if (m_WorldDialogCount != int.MaxValue)
            {
                throw new StackOverflowException($"Amount of Dialogs exceeded the capacity of {int.MaxValue}");
            }
            Debug.LogError("Dialogs exceeded max amount. Returning garbage.");
            return false;
        }
        return true;
    }
    private void BeginPrintingText()
    {
        if (m_PrintingText)
        {
            StopCoroutine(m_TextPrinter);
        }
        m_TextPrinter = PrintText();
        StartCoroutine(m_TextPrinter);
    }
    private int FetchID()
    {
        int id = 0;
        while (!m_WorldDialogIDs.Contains(id))
        {
            id++;
        }
        return id;
    }

    private void UpdateWorldDialog(WorldDialogData dialogData)
    {
        
        //Check if should still exist
        //if should
            //Move dialog
        //else
            //Remove dialog
    }
    
    //private bool IsMainDialogFinishedPrinting()
    //{
    //    return m_PrintingText && ;
    //}

    private void UpdateMainDialog()
    {
        if (m_CurrentMainDialog != null)
        {
            if (m_CurrentMainDialog.TimeLeftToDisplay < 0f || (m_QueuedMainDialogs.Count > 0 && m_CurrentMainDialog.TimeLeftToDisplay == float.MaxValue && Time.time > m_CurrentMainDialog.TimeAtDisplay + m_PrintFinishedBuffer && m_PrintingText == false))
            {
                GoToNextDialogIfPossible();
            }
        }
    }

    private void GoToNextDialogIfPossible()
    {
        if (m_CurrentMainDialog != null)
        {
            m_CurrentMainDialog.Hide();
            m_CurrentMainDialog = null;
            OnDialogDone?.Invoke();

            if (m_QueuedMainDialogs.Count > 0)
            {
                m_CurrentMainDialog = m_QueuedMainDialogs.Dequeue();
                m_CurrentMainDialog.Display();

                BeginPrintingText();
            }
        }
    }
}
