using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeHUDManager : MonoBehaviour
{
    [Header("UpgradeButtonManagers")]
    [SerializeField] private UpgradeButtonManager m_UpgradeButtonManagerLeft;
    [SerializeField] private UpgradeButtonManager m_UpgradeButtonManagerRight;
    [SerializeField] private GameObject m_UpgradeRoot = null;
    [SerializeField] private GameObject PickUpPanel;
    //[SerializeField] private Text m_PickupText;
    [SerializeField] private SpellUpgradeDataHolder m_SpellUpgradeDataHolder = null;

    [SerializeField] private PlayerController m_Player;

    public PlayerController Player => m_Player;

    public void EnableUpgradePanels(SpellUpgradeData[] UpgradeArray)
    {
        if (UpgradeArray.Length < 1)
            return;
        
        m_UpgradeButtonManagerLeft.SetButtonData(m_SpellUpgradeDataHolder, UpgradeArray[0], m_Player, UpgradePlacementType.Left);

        if (UpgradeArray.Length > 1)
            m_UpgradeButtonManagerRight.SetButtonData(m_SpellUpgradeDataHolder, UpgradeArray[1], m_Player, UpgradePlacementType.Right);
        else
            m_UpgradeButtonManagerRight.SetButtonData(m_SpellUpgradeDataHolder, UpgradeArray[0], m_Player, UpgradePlacementType.Left);
        m_UpgradeRoot.SetActive(true);

        Time.timeScale = float.Epsilon;

    }
    public void DisableUpgradePanels()
    {
        m_UpgradeRoot.SetActive(false);
        m_Player.RestoreHealth(100f);
        Time.timeScale = 1f;
    }

    public void TogglePickUpPanel(bool enabled, string TextPickUpString = "Spell Upgrade")
    {
        PickUpPanel.SetActive(enabled);
        //m_PickupText.text = TextPickUpString;
    }


    public void TogglePickUpPanel(bool enabled)
    {
        PickUpPanel.SetActive(enabled);
    }
}
