using System.Collections.Generic;
using UnityEngine;
public class ElementReactant : MonoBehaviour, IElementReactant<KeyValuePair<ElementType, ElementState>>
{ 
    public FresnelSettingsDataHolder FresnelSettingsDataHolder;
    private ElementState _currentElement;

    [SerializeField] private ElementState _standardElement;
    [SerializeField] private GameObject[] m_FresnelMaterialUsers;
    [SerializeField] private Transform m_FirePfx;
    [SerializeField] private Transform m_LightningPFX;
    [SerializeField] private Transform m_IcePfx;
    [SerializeField] private Transform m_PoisonPfx;
    [SerializeField] private Transform m_PoisonFirePfx;
    private Transform m_CurrentPfx;
    private Dictionary<ElementType, Transform> m_TypePFXDictionary = new Dictionary<ElementType, Transform>();
    private Material[] m_fresnelMaterials;
    private Material BodyMaterial => m_fresnelMaterials[0];
    private Material HeadMaterial => m_fresnelMaterials[1];
    private float m_ElementRemovalTime;
    private const string k_FresnelColor = "_FresnelColor";
    private const string k_FresnelMultiply = "_FresnelMultiply";
    private const string k_FresnelPower = "_FresnelPower";



    public ElementState CurrentElementState { 
        get => _currentElement ? _currentElement : _standardElement; 
        set 
        {
            if (value == null)
            {
                value = _standardElement;
            }
            //This is hella messy, and was added really late. Basicly, if current element wasnt null, or wasnt refreshed, and it wasn't playing,
            if (m_TypePFXDictionary.Count > 0) {
                if (_currentElement != null &&  
                    _currentElement.elementType != value.elementType && 
                    m_CurrentPfx != null && 
                    _currentElement.elementType != ElementType.Neutral)
                {
                        for (int i = 0; i < m_CurrentPfx.childCount; i++)
                            m_CurrentPfx.GetChild(i).GetComponent<ParticleSystem>().Stop();
                }
                if(value.elementType != ElementType.Neutral)
                    if (_currentElement == null || 
                        _currentElement.elementType != value.elementType )
                    {
                        m_CurrentPfx = m_TypePFXDictionary[value.elementType];
                        m_CurrentPfx?.GetComponent<ParticleSystem>().Play();
                    }
            }
            _currentElement = value; 
            elementStateUpdated?.Invoke(value);
            m_ElementRemovalTime = Time.time + value.duration;
            
            //Update FresnelSettings if possible
            if(m_fresnelMaterials == null)
            {
                return;
            }

            BodyMaterial.SetColor(k_FresnelColor, FresnelSettingsDataHolder.GetFresnelSettingsStruct(value.elementType).FresnelColor);
            HeadMaterial.SetColor(k_FresnelColor, FresnelSettingsDataHolder.GetFresnelSettingsStruct(value.elementType).FresnelColor);

            HeadMaterial.SetFloat(k_FresnelPower, FresnelSettingsDataHolder.GetFresnelSettingsStruct(value.elementType).HeadPower);
            HeadMaterial.SetFloat(k_FresnelMultiply, FresnelSettingsDataHolder.GetFresnelSettingsStruct(value.elementType).HeadMultiplier);

            BodyMaterial.SetFloat(k_FresnelPower, FresnelSettingsDataHolder.GetFresnelSettingsStruct(value.elementType).Bodypower);
            BodyMaterial.SetFloat(k_FresnelMultiply, FresnelSettingsDataHolder.GetFresnelSettingsStruct(value.elementType).BodyMultiplier);
        }
    }
    private delegate void ElementStateUpdateDelegate(ElementState newElement);
    private ElementStateUpdateDelegate elementStateUpdated;
    private delegate void ElementUpdateDelegate(ElementState elementState);

    public void Awake()
    {
        //Hardcoded to avoid Mishaps
        m_TypePFXDictionary.Add(ElementType.Fire, m_FirePfx);
        m_TypePFXDictionary.Add(ElementType.Ice, m_IcePfx);
        m_TypePFXDictionary.Add(ElementType.Lightning, m_IcePfx);
        m_TypePFXDictionary.Add(ElementType.Poison, m_PoisonPfx);
        m_TypePFXDictionary.Add(ElementType.PoisonFire, m_PoisonFirePfx);
        
        //Subscribe ElementStateUpdate
        if(TryGetComponent<IElementAffectable>(out IElementAffectable elementAffectable))
        {
            elementStateUpdated += elementAffectable.SetElement;
        }

        m_fresnelMaterials = new Material[m_FresnelMaterialUsers.Length];
        for (int i = 0; i < m_FresnelMaterialUsers.Length; i++)
        {
            Material tempMat = m_FresnelMaterialUsers[i].GetComponent<SkinnedMeshRenderer>().materials[0];
            m_fresnelMaterials[i] = tempMat;
            m_FresnelMaterialUsers[i].GetComponent<SkinnedMeshRenderer>().materials[0] = m_fresnelMaterials[i];
        }

        if (_standardElement)
            CurrentElementState = _standardElement;
    }
    public void Update()
    {
        TickElement();
    }

    private void TickElement()
    {   
        if (_currentElement == _standardElement)
        {
            return;
        }

        if (m_ElementRemovalTime < Time.time)
        {
            CurrentElementState = _standardElement;
        }
    }
    public void TryAffect(KeyValuePair<ElementType, ElementState> element, out bool affected)
    {
        affected = CurrentElementState.elementType.Equals(element.Key);
        if (affected == true)
        {
            CurrentElementState = element.Value;
            return;
        }
        foreach (ElementType et in CurrentElementState.ignoredReactants)
        {
            if (et.Equals(element.Value.elementType))
                return;
        }
        foreach(ElementType et in CurrentElementState.refreshingReactants)
        {
            if (et.Equals(element.Value.elementType))
            {
                CurrentElementState = CurrentElementState;
                affected = true;
                return;
            }
        }
    }
    void OnDisable()
    {
        CurrentElementState = _standardElement;
    }
}
