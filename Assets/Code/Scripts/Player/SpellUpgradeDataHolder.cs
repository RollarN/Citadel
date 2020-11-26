using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeHoverType
{
    None,
    Hover
}
public enum UpgradePlacementType
{
    Left,
    Right
}
[CreateAssetMenu (fileName ="SpellUpgrade Dataholder", menuName = "UpgradeSystem/DataHolder")]
public class SpellUpgradeDataHolder : ScriptableObject
{
    public Sprite FireIcon;

    public Sprite Poisoncon;

    public Sprite LightningIcon;

    public Sprite IceIcon;
    [SerializeField] private Sprite m_FireSpriteLeft = null;
    [SerializeField] private Sprite m_FireSpriteRight = null;
    [SerializeField] private Sprite m_FireSpriteHoverLeft = null;
    [SerializeField] private Sprite m_FireSpriteHoverRight = null;
    [SerializeField] private Sprite m_IceSpriteLeft = null;
    [SerializeField] private Sprite m_IceSpriteRight = null;
    [SerializeField] private Sprite m_IceSpriteHoverLeft = null;
    [SerializeField] private Sprite m_IceSpriteHoverRight = null;
    [SerializeField] private Sprite m_LightningLeft = null;
    [SerializeField] private Sprite m_LightningRight = null;
    [SerializeField] private Sprite m_LightningHoverLeft = null;
    [SerializeField] private Sprite m_LightningHoverRight = null;
    [SerializeField] private Sprite m_OilSpriteLeft = null;
    [SerializeField] private Sprite m_OilSpriteRight = null;
    [SerializeField] private Sprite m_OilSpriteHoverLeft = null;
    [SerializeField] private Sprite m_OilSpriteHoverRight = null;

    public List<SpellUpgradeData> GetUpgradeList(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire: return FireUpgrades;
            case ElementType.Ice: return FrostUpgrades;
            case ElementType.Lightning: return LightningUpgrades;
            case ElementType.Poison: return OilUpgrades;
        }
        Debug.LogWarning("Index out of range");
        return null;
    }

    public Sprite GetUpgradeSprite(ElementType elementType, UpgradeHoverType hoverType, UpgradePlacementType placement)
    {
        if (placement == UpgradePlacementType.Left)
        {
            switch (elementType)
            {
                case ElementType.Fire:
                    return hoverType == UpgradeHoverType.Hover ? m_FireSpriteHoverLeft : m_FireSpriteLeft;
                case ElementType.Poison:
                    return hoverType == UpgradeHoverType.Hover ? m_OilSpriteHoverLeft : m_OilSpriteLeft;
                case ElementType.Ice:
                    return hoverType == UpgradeHoverType.Hover ? m_IceSpriteHoverLeft : m_IceSpriteLeft;
                case ElementType.Lightning:
                    return hoverType == UpgradeHoverType.Hover ? m_LightningHoverLeft : m_LightningLeft;
            }
        }
        else
        {
            switch (elementType)
            {
                case ElementType.Fire:
                    return hoverType == UpgradeHoverType.Hover ? m_FireSpriteHoverRight : m_FireSpriteRight;
                case ElementType.Poison:
                    return hoverType == UpgradeHoverType.Hover ? m_OilSpriteHoverRight : m_OilSpriteRight;
                case ElementType.Ice:
                    return hoverType == UpgradeHoverType.Hover ? m_IceSpriteHoverRight : m_IceSpriteRight;
                case ElementType.Lightning:
                    return hoverType == UpgradeHoverType.Hover ? m_LightningHoverRight : m_LightningRight;
            }
        }
        return null;
    }
    public Sprite GetUpgradeIcon(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Poison:
                return Poisoncon;
            case ElementType.Fire:
                return FireIcon;
            case ElementType.Ice:
                return IceIcon;
            case ElementType.Lightning:
                return LightningIcon;
        }
        return null;
    }
    public List<SpellUpgradeData> FireUpgrades;
    public List<SpellUpgradeData> FrostUpgrades;
    public List<SpellUpgradeData> LightningUpgrades;
    public List<SpellUpgradeData> OilUpgrades;
}
