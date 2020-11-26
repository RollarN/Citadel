using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogSender : MonoBehaviour, ISavable
{
    [SerializeField] private DialogManager m_DialogManager = null;
    [SerializeField] private Sprite m_Image = null;
    [SerializeField] private string m_Title = string.Empty;
    [SerializeField] private string m_Subtitle = string.Empty;
    [SerializeField] private string m_Content = string.Empty;
    [SerializeField] private float m_TimeAlive = 0f;
    [SerializeField] private bool m_OverrideActiveDialog = false;

    private LayerMask m_PlayerLayer;

    public bool ObjectActiveOnSave { get; set; }

    public void OnLoad(string[] savedData, int startIndex)
    {
        
    }

    public void OnSave(StreamWriter sw, out int addedCount)
    {
        gameObject.SetActive(ObjectActiveOnSave);
        addedCount = 0;
    }

    private void Awake()
    {
        m_PlayerLayer = LayerMask.NameToLayer("PlayerLayer");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == m_PlayerLayer)
        {
            m_DialogManager.SendMainDialog(m_Image, m_Title, m_Subtitle, m_Content, m_TimeAlive, m_OverrideActiveDialog);
            gameObject.SetActive(false);
        }
    }
}
