using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "new player projectile", menuName = "Projectiles/PlayerProjectileData")]
public class PlayerSpellData : SpellData
{
    [Header("Playerspecific Data")]

    public Dictionary<SpellUpgradeData, uint> UpgradeLevels = new Dictionary<SpellUpgradeData, uint>();

    [SerializeField] public AnimationCurve ChargeCurve;
    [Header("Player Specific Variables")]
    [HideInInspector] private const float m_MaxAmmoPercent = 100;
    [Tooltip("How many percent are recovered per second")]
    [SerializeField] private float m_AmmoRechargePercentPerSec = 2;
    [Tooltip("Charge used for every shot")]
    [SerializeField] private float m_ResourceCostPerShot = 50;
    public float ResourceCostPerShot => m_ResourceCostPerShot;
    private float m_CurrentChargePercent = 100;
    [HideInInspector]
    public float CurrentChargePercent
    {
        get => m_CurrentChargePercent;
        set
        {
            m_CurrentChargePercent = Mathf.Clamp(value, 0, 100);
        }
    }
    public float Recharge()
    {
        CurrentChargePercent += Time.deltaTime * m_AmmoRechargePercentPerSec;
        return CurrentChargePercent;
    }



    #region UpgradeSystem
    [HideInInspector] public int ElementLevel;
    public float AverageDamageOverTimeEffects
    {
        get
        {
            float totalDamageOverTimeEffects = 0;
            List<ElementState> foundElementStates = new List<ElementState>();
            foreach (ElementState ES in ImpactElementState.Values)
            {
                if (!foundElementStates.Contains(ES))
                    if (ES.EffectsPerSecond.ContainsKey(ElementEffect.TakeDamage))
                    {
                        foundElementStates.Add(ES);
                        totalDamageOverTimeEffects += ES.EffectsPerSecond[ElementEffect.TakeDamage];
                    }
            }
            return totalDamageOverTimeEffects /= foundElementStates.Count;
        }
        set
        {
            float preUpgradeValue = AverageDamageOverTimeEffects;
            List<ElementState> foundElementStates = new List<ElementState>();
            foreach (ElementState ES in ImpactElementState.Values)
            {
                if (!foundElementStates.Contains(ES))
                    if (ES.EffectsPerSecond.ContainsKey(ElementEffect.TakeDamage))
                    {
                        foundElementStates.Add(ES);
                        ES.EffectsPerSecond[ElementEffect.TakeDamage] = value;
                    }
            }
        }
    }

    public float AverageStunEffects
    {
        get
        {
            float totalStunEffects = 0;
            List<ElementState> foundElementStates = new List<ElementState>();
            foreach (ElementState ES in ImpactElementState.Values)
            {
                if (!foundElementStates.Contains(ES))
                    if (ES.EffectsWhenApplied.ContainsKey(ElementEffect.Stun))
                    {
                        foundElementStates.Add(ES);
                        totalStunEffects += ES.EffectsWhenApplied[ElementEffect.Stun];
                    }
            }
            return totalStunEffects /= foundElementStates.Count;
        }
        set
        {
            List<ElementState> foundElementStates = new List<ElementState>();
            foreach (ElementState ES in ImpactElementState.Values)
            {
                if (!foundElementStates.Contains(ES))
                    if (ES.EffectsWhenApplied.ContainsKey(ElementEffect.Stun))
                    {
                        foundElementStates.Add(ES);
                        ES.EffectsWhenApplied[ElementEffect.Stun] = value;
                    }
            }
        }
    }
    public float AverageSlowEffects
    {
        get
        {
            float totalSlowEffects = 0;
            List<ElementState> foundElementStates = new List<ElementState>();
            foreach (ElementState ES in ImpactElementState.Values)
            {
                if (!foundElementStates.Contains(ES))
                    if (ES.EffectsWhenApplied.ContainsKey(ElementEffect.UpdateMovementSpeed))
                    {
                        foundElementStates.Add(ES);
                        totalSlowEffects += ES.EffectsWhenApplied[ElementEffect.UpdateMovementSpeed];
                    }
            }
            return totalSlowEffects /= foundElementStates.Count;
        }

        set
        {
            List<ElementState> foundElementStates = new List<ElementState>();
            foreach (ElementState ES in ImpactElementState.Values)
            {
                if (!foundElementStates.Contains(ES))
                    if (ES.EffectsWhenApplied.ContainsKey(ElementEffect.UpdateMovementSpeed))
                    {
                        foundElementStates.Add(ES);
                        ES.EffectsWhenApplied[ElementEffect.UpdateMovementSpeed] = value;
                    }
            }
        }
    }


    public float GetStatValue(SpellUpgradeType type)
    {
        switch (type)
        {
            case SpellUpgradeType.ImpactDamage:
                return Damage;
            case SpellUpgradeType.TravelSpeed:
                return TravelSpeed;
            case SpellUpgradeType.ImpactRadius:
                return ImpactRadius;
            case SpellUpgradeType.ProjectileRadius:
                return Radius;
            case SpellUpgradeType.DamageOverTime:
                return AverageDamageOverTimeEffects;
            case SpellUpgradeType.StunTime:
                return AverageStunEffects;
            case SpellUpgradeType.Slow:
                return AverageSlowEffects;
        }
        Debug.LogError("UpgradeSystem: No float matching upgradetype found.");
        return 0;
    }
    public void UpgradeStat(SpellUpgradeType type, float magnitude)
    {
        switch (type)
        {
            case SpellUpgradeType.ImpactDamage:
                Damage += magnitude;
                break;
            case SpellUpgradeType.TravelSpeed:
                TravelSpeed += magnitude;
                break;
            case SpellUpgradeType.ImpactRadius:
                ImpactRadius += magnitude;
                break;
            case SpellUpgradeType.ProjectileRadius:
                Radius += magnitude;
                break;
            case SpellUpgradeType.DamageOverTime:
                AverageDamageOverTimeEffects += magnitude;
                break;
            case SpellUpgradeType.StunTime:
                AverageStunEffects += magnitude;
                break;
            case SpellUpgradeType.Slow:
                AverageSlowEffects += magnitude;
                break;
        }
    }
    #endregion UpgradeSystem
}
