using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpellUpgradePickup : MonoBehaviour
{
    public SpellUpgradeDataHolder dataholder;
    public bool m_Used = false;
    public bool Used
    {
        get => m_Used;
        set
        {
            m_Used = value;
            if(transform.childCount > 0)
            {
                if(transform.GetChild(0).childCount > 0)
                {
                    for(int i = 0; i < transform.GetChild(0).childCount; i++)
                    {
                        transform.GetChild(0).GetChild(i).gameObject.SetActive(!value);
                    }
                }
            }
        }
    }

    public bool GetSpellUpgrades(PlayerController player, out int UpgradesCount, out SpellUpgradeData[] UpgradeArray)
    {
        UpgradeArray = new SpellUpgradeData[2] { null, null };
        UpgradesCount = 0;
        if (m_Used)
            return false;

        List<int> EmptyListIndex = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            List<SpellUpgradeData> UpgradeList = dataholder.GetUpgradeList((ElementType)i);

            for(int j = 0; j < UpgradeList.Count; j++)
            {
                    if (!UpgradeList[j].IsUpgradable(player))
                        UpgradeList.Remove(UpgradeList[j]);
            }
            if (UpgradeList.Count <= 0)
                EmptyListIndex.Add(i);
        }

        UpgradesCount = 4 - EmptyListIndex.Count;
        if (UpgradesCount == 0)
            return false;

        int firstElementTypeIndex;
        do
        {
            firstElementTypeIndex = Random.Range(0, 4);

        } while (EmptyListIndex.Contains(firstElementTypeIndex));

        int secondElementTypeIndex = 0;
        if (UpgradesCount > 1)
        {
            do
            {
                secondElementTypeIndex = Random.Range(0, 4);
            }
            while (secondElementTypeIndex == firstElementTypeIndex || EmptyListIndex.Contains(secondElementTypeIndex));
        }

        for (int i = 0; i < Mathf.Min(2, UpgradesCount); i++) {
            List<SpellUpgradeData> UpgradeList = dataholder.GetUpgradeList(i == 0 ? (ElementType)firstElementTypeIndex : (ElementType)secondElementTypeIndex);
            do
            {
                int index = Random.Range(0, UpgradeList.Count - 1);
                if (UpgradeList[index].IsUpgradable(player))
                    UpgradeArray[i] = UpgradeList[index];
            } while (UpgradeArray[i] == null);
        }
        return true;
    }
}
