using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainDialogData
{
    private readonly DialogManager m_DialogManager = null;
    private readonly GameObject m_DialogRoot = null;
    private readonly Sprite m_Sprite = null;
    private readonly string m_Title = string.Empty;
    private readonly string m_Subtitle = string.Empty;
    private readonly string m_Content = string.Empty;
    private readonly float m_DisplayTime = 0f;
    private float m_TimeAtDisplay = 0f;
    private bool m_IsDialogDisplayed = false;

    public Sprite Sprite => m_Sprite;
    public string Title => m_Title;
    public string Subtitle => m_Subtitle;
    public string Content => m_Content;
    public float DisplayTime => m_DisplayTime;
    public float TimeAtDisplay => m_TimeAtDisplay;
    public float TimeLeftToDisplay => m_DisplayTime == 0 ? float.MaxValue : m_DisplayTime - (Time.time - m_TimeAtDisplay);
    public bool IsDialogDisplayed => m_IsDialogDisplayed;
    
    public MainDialogData(DialogManager dialogManager, GameObject dialogRoot, Sprite sprite, string title, string subtitle, string content, float displayTime)
    {
        m_DialogManager = dialogManager;
        m_DialogRoot = dialogRoot;
        m_Sprite = sprite;
        m_Title = title;
        m_Subtitle = subtitle;
        m_Content = content;
        m_DisplayTime = displayTime;
    }

    public void Display()
    {
        m_TimeAtDisplay = Time.time;
        m_IsDialogDisplayed = true;
        m_DialogManager.MainDialogTitleText.text = m_Title;
        m_DialogManager.MainDialogSubtitleText.text = m_Subtitle;
        
        if(m_Sprite != null)
        {
            m_DialogManager.MainDialogImage.sprite = m_Sprite;
            m_DialogManager.MainDialogImage.gameObject.SetActive(true);
        }
        else
        {
            m_DialogManager.MainDialogImage.gameObject.SetActive(false);
        }

        m_DialogRoot.SetActive(true);
    }

    public void Hide()
    {
        m_IsDialogDisplayed = false;
        m_DialogRoot.SetActive(false);
    }
}
