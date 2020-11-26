using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName ="New SpellUpgradePickup", menuName = "UpgradeSystem/SpellUpgradePickupData")]
public class SpellUpgradeData : ScriptableObject
{

    public float Magnitude;
    public ElementType ElementType = default;
    public SpellUpgradeType UpgradeType = default;

    [SerializeField] private uint m_ElementLevelRequirement = 0;
    [SerializeField] private uint m_MaxUpgradeLevel = 0;

    public string UpgradeName
    {
        get => UpgradeType.ToString();
    }

    public bool IsUpgradable(PlayerController player)
    {
       if (player.SpellDataDictionary[ElementType].ElementLevel >= m_ElementLevelRequirement)
            {
                if (m_MaxUpgradeLevel > 0)
                {
                    if (player.SpellDataDictionary[ElementType].UpgradeLevels.ContainsKey(this))
                        return player.SpellDataDictionary[ElementType].UpgradeLevels[this] < m_MaxUpgradeLevel;
                }
                else
                    return true;
            }
            return false;
        
    }

    public float UpgradePercent(PlayerController player)
    {
            float BaseNumber = player.SpellDataDictionary[ElementType].GetStatValue(UpgradeType);
            return (((BaseNumber + Magnitude) / BaseNumber) - 1f) * 100f;
        
    }


    public string UpgradeString(PlayerController player)
    {
        return ElementType.ToString() + "ball \n" + UpgradeType.ToString() + " +" + UpgradePercent(player) +"%";
    }
    public void UpgradeSpell(PlayerController player)
    {
        player.SpellDataDictionary[ElementType].UpgradeStat(UpgradeType, Magnitude);
        if (m_MaxUpgradeLevel > 0)
            player.SpellDataDictionary[ElementType].UpgradeLevels[this]++;
    }
}
