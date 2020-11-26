using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Element/ElementFresnelSettings")]
public class FresnelSettingsDataHolder : ScriptableObject
{
    [SerializeField] public FresnelSettings FireFresnelSettings;
    [SerializeField] public FresnelSettings PoisonFresnelSettings;
    [SerializeField] public FresnelSettings IceFresnelSettings;
    [SerializeField] public FresnelSettings LightningFresnelSettings;
    [SerializeField] public FresnelSettings NeutralFresnelSettings;

    [System.Serializable]
    public struct FresnelSettings
    {
        public Color FresnelColor;
        public float HeadPower;
        public float HeadMultiplier;
        public float Bodypower;
        public float BodyMultiplier;
    }
    public FresnelSettings GetFresnelSettingsStruct(ElementType elementType)
    {
        switch(elementType)
        {
            case ElementType.Fire:
                return FireFresnelSettings;
            case ElementType.Ice:
                return IceFresnelSettings;
            case ElementType.PoisonFire:
            case ElementType.Poison:
                return PoisonFresnelSettings;
            case ElementType.Lightning:
                return LightningFresnelSettings;
            default:
                return NeutralFresnelSettings;
        }
    }

}
