using UnityEngine;
using System;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Element/ElementState")]
public class ElementState : ScriptableObject
{

    public float duration;
    public ElementType elementType;
    public ElementType[] ignoredReactants;
    public ElementType[] refreshingReactants;

    [SerializeField] private SpellVFXPools m_VFXPools;

    public EffectEnumFloatDictionary EffectsWhenApplied;
    [FormerlySerializedAs("EffectsOnUpdate")]
    public EffectEnumFloatDictionary EffectsPerSecond;
    public EffectEnumFloatDictionary EffectsWhenRemoved;
}
[System.Serializable]
public class EffectEnumFloatDictionary : SerializableDictionary<ElementEffect, float>
{

}
