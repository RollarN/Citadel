using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public Text ElementText = null;
    public Image UpgradeImage = null;
    public Image UpgradeImageHover = null;
    public Text PercentUpgradeText = null;
    public Text CurrentUpgradeLevel = null;
    public Text CurrentElementUpgrades = null;
    public Image UpgradeIcon = null;

    [SerializeField] private float m_HoverImageSwapDuration = 1f;
    private float m_NormalSpriteOpacity = 1f;
    private float m_HoverSpriteOpacity = 0f;
    private bool m_IsLerping = false;
    private bool m_LerpToHover = false;

    //public HUDManager HUDManager;
    
    [SerializeField] private UpgradeHUDManager m_UpgradeHudManager = null;


    [HideInInspector] public SpellUpgradeData spellUpgradeData;

    private void Awake()
    {
        UpgradeImage.alphaHitTestMinimumThreshold = 0.5f;
        UpgradeImageHover.alphaHitTestMinimumThreshold = 0.5f;
        UpgradeImageHover.color = new Color(UpgradeImageHover.color.r, UpgradeImageHover.color.g, UpgradeImageHover.color.b, 0);

        UpgradeIcon.color = new Color(UpgradeIcon.color.r, UpgradeIcon.color.g, UpgradeIcon.color.b, 0);
        ElementText.color = new Color(ElementText.color.r, ElementText.color.g, ElementText.color.b, 0);
        PercentUpgradeText.color = new Color(PercentUpgradeText.color.r, PercentUpgradeText.color.g, PercentUpgradeText.color.b, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
        m_LerpToHover = true;
        m_IsLerping = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_LerpToHover = false;
        m_IsLerping = true;
        Debug.Log("Exit");
    }

    public void SetButtonData(SpellUpgradeDataHolder spellUpgradeDataHolder, SpellUpgradeData newSpellUpgradeData, PlayerController player, UpgradePlacementType placement)
    {
        spellUpgradeData = newSpellUpgradeData;
        ElementText.text = spellUpgradeData.ElementType.ToString();
        UpgradeImage.sprite = spellUpgradeDataHolder.GetUpgradeSprite(newSpellUpgradeData.ElementType, UpgradeHoverType.None, placement);
        UpgradeIcon.sprite = spellUpgradeDataHolder.GetUpgradeIcon(newSpellUpgradeData.ElementType);
        UpgradeImageHover.sprite = spellUpgradeDataHolder.GetUpgradeSprite(newSpellUpgradeData.ElementType, UpgradeHoverType.Hover, placement);
        PercentUpgradeText.text = newSpellUpgradeData.UpgradeName; //  + " +" + newSpellUpgradeData.UpgradePercent(player).ToString("0.0") + "%";
        CurrentElementUpgrades.text = "Current " + newSpellUpgradeData.ElementType + " Level - "  + player.SpellDataDictionary[newSpellUpgradeData.ElementType].ElementLevel;
        if (player.SpellDataDictionary[newSpellUpgradeData.ElementType].UpgradeLevels.TryGetValue(newSpellUpgradeData, out uint upgradeLevel))
            CurrentUpgradeLevel.text = "Current " + newSpellUpgradeData.UpgradeName + "Level - " + upgradeLevel;
        else CurrentUpgradeLevel.text = "";
    }
    public void UpgradeSpell()
    {
        
        spellUpgradeData.UpgradeSpell(m_UpgradeHudManager.Player);
        m_UpgradeHudManager.DisableUpgradePanels();
    }

    private void Update()
    {
        if (m_IsLerping)
        {
            LerpToImage(m_LerpToHover);
        }
    }

    private void LerpToImage(bool hover)
    {
        if (hover)
        {
            if (m_HoverSpriteOpacity < 1f)
            {
                m_HoverSpriteOpacity = Mathf.Clamp01(m_HoverSpriteOpacity + (Time.unscaledDeltaTime / m_HoverImageSwapDuration));
                m_NormalSpriteOpacity = Mathf.Clamp01(1 - m_HoverSpriteOpacity);

                UpgradeImage.color = new Color(UpgradeImage.color.r, UpgradeImage.color.g, UpgradeImage.color.b, m_NormalSpriteOpacity);
                UpgradeImageHover.color = new Color(UpgradeImageHover.color.r, UpgradeImageHover.color.g, UpgradeImageHover.color.b, m_HoverSpriteOpacity);

                UpgradeIcon.color = new Color(UpgradeIcon.color.r, UpgradeIcon.color.g, UpgradeIcon.color.b, m_HoverSpriteOpacity);
                ElementText.color = new Color(ElementText.color.r, ElementText.color.g, ElementText.color.b, m_HoverSpriteOpacity);
                PercentUpgradeText.color = new Color(PercentUpgradeText.color.r, PercentUpgradeText.color.g, PercentUpgradeText.color.b, m_HoverSpriteOpacity);
            }
            else
            {
                m_HoverSpriteOpacity = 1f;
                m_NormalSpriteOpacity = 0f;
            }
        }
        else
        {
            if (m_NormalSpriteOpacity < 1f)
            {
                m_NormalSpriteOpacity = Mathf.Clamp01(m_NormalSpriteOpacity + (Time.unscaledDeltaTime / m_HoverImageSwapDuration));
                m_HoverSpriteOpacity = 1 - m_NormalSpriteOpacity;

                UpgradeImage.color = new Color(UpgradeImage.color.r, UpgradeImage.color.g, UpgradeImage.color.b, m_NormalSpriteOpacity);
                UpgradeImageHover.color = new Color(UpgradeImageHover.color.r, UpgradeImageHover.color.g, UpgradeImageHover.color.b, m_HoverSpriteOpacity);

                UpgradeIcon.color = new Color(UpgradeIcon.color.r, UpgradeIcon.color.g, UpgradeIcon.color.b, m_HoverSpriteOpacity);
                ElementText.color = new Color(ElementText.color.r, ElementText.color.g, ElementText.color.b, m_HoverSpriteOpacity);
                PercentUpgradeText.color = new Color(PercentUpgradeText.color.r, PercentUpgradeText.color.g, PercentUpgradeText.color.b, m_HoverSpriteOpacity);
            }
            else
            {
                m_NormalSpriteOpacity = 1f;
                m_HoverSpriteOpacity = 0f;
            }
        }

        
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        UpgradeSpell();
    }
}
