using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectiles/ProjectileData")]
public class SpellData : ScriptableObject
{   [Header("References")]
    [SerializeField] private SpellType m_SpellType = default;
    [SerializeField] private SpellVFXPools m_VFXPools = null;
    [SerializeField] private ElementType m_ElementType = default;
    [SerializeField] private LayerMask m_CollisionLayer;
    [SerializeField] private LayerMask m_PassThroughExplosiveLayer;

    [Header("SFX")]
    [SerializeField] private AudioClip m_ShootingSound;
    [SerializeField] private AudioClip m_ImpactSound;
    [SerializeField] private AudioClip m_ChargingSound;
    [SerializeField] private TargetAudioMixer m_TargetAudioMixer;

    [Header("ImpactStates")]
    [SerializeField] protected EnumElementStateDictionary m_ImpactElementState;
    [SerializeField]
    public IDictionary<ElementType, ElementState> ImpactElementStateIDictionary { get { return m_ImpactElementState; } set { m_ImpactElementState.CopyFrom(value); } }
    [Header("ProjectilePhase")]
    [SerializeField] private float m_Distance;
    [SerializeField] private float m_TravelSpeed;
    [SerializeField] private float m_CollisionRadius;

    [Header("ImpactPhase")]
    [SerializeField] private float m_ImpactEffectRadius = 0.1f;
    [SerializeField] private float m_FiringCooldown = 0.05f;
    [SerializeField] private float m_Damage;

    [Tooltip("Determines fallof of damage by distance")]
    [SerializeField] private AnimationCurve DamageByDistance;

    [SerializeField] private float m_ImpactExplosionForce = 20;
    [Header("Charging")]

    [SerializeField] protected float m_TimeToReachFullCharge = 1f;

    [SerializeField] protected float m_MinimumChargeTime = 0.5f;
    [SerializeField] [Range(0f, 1f)] protected float m_ChargingMovementSpeedDividend = 1f;

    #region Properties
    public SpellType SpellType => m_SpellType;
    public ElementType ElementType => m_ElementType;
    public float TravelSpeed { get => m_TravelSpeed; set => m_TravelSpeed = value; }
    public float ImpactRadius { get => m_ImpactEffectRadius; set => m_CollisionRadius = value; }
    public float Radius { get => m_CollisionRadius; set => m_CollisionRadius = value; }
    public float FiringCooldown { get => m_FiringCooldown; }
    public float Distance { get => m_Distance; set => m_Distance = value; }
    public float Damage { get => m_Damage; set => m_Damage = value; }
    public float TimeToReachFullCharge => m_TimeToReachFullCharge;
    public LayerMask CollisionLayer => m_CollisionLayer + LayerMask.GetMask("ObstacleLayer");
    public EnumElementStateDictionary ImpactElementState { get => m_ImpactElementState; set => m_ImpactElementState = value; }
    public float MinimumChargeTime => m_MinimumChargeTime;
    public AudioClip ChargingSound => m_ChargingSound;
    public AudioClip ShootingSound => m_ShootingSound;
    public float ChargingMovementSpeedDividend => m_ChargingMovementSpeedDividend;
    //Rescales spells so that they are visually as big as their collision size
    public Vector3 VisualProjectileRadiusScaledByElementType => Vector3.one * m_CollisionRadius * 1.4f;
    public Vector3 VisualImpactRadiusScaledByElementType
    {
        get
        {
            switch (m_ElementType)
            {
                case ElementType.Poison:
                    return Vector3.one * m_ImpactEffectRadius;
                case ElementType.Fire:
                    return Vector3.one * m_ImpactEffectRadius;
                case ElementType.Ice:
                    return Vector3.one * m_ImpactEffectRadius * 1 / 3f;
                default:
                    return Vector3.one;
            }
        }
    }

    #endregion Properties

    public void UpdateProjectile(Projectile projectile, GameObject sender, float charge = 1)
    {
        Vector3 halfExtents = new Vector3(m_CollisionRadius, 0.05f, m_CollisionRadius);

        if (Physics.BoxCast(projectile.transform.position, halfExtents, projectile.transform.forward, out RaycastHit hit, projectile.transform.rotation, m_TravelSpeed * Time.deltaTime * charge, m_CollisionLayer))
        {
            TriggerSpellImpact(hit.point - (Vector3.up * 0.2f), sender, charge, projectile.transform);
            projectile.gameObject.SetActive(false);
            return;
        }

        else
            projectile.transform.Translate(projectile.transform.forward * m_TravelSpeed * Time.deltaTime * charge, Space.World);

        int AmountOfOverlappingProps = Physics.OverlapBoxNonAlloc(projectile.transform.position, Vector3.one * m_CollisionRadius, projectile.ExplosivePropOverLapBuffer, projectile.transform.rotation, m_PassThroughExplosiveLayer);
        if (AmountOfOverlappingProps > 0)
        {
            for (int i = 0; i < AmountOfOverlappingProps; i++)
                projectile.ExplosivePropOverLapBuffer[i].GetComponent<IDestructible>()?.Explode(m_ImpactExplosionForce, projectile.transform.position, m_CollisionRadius, 0);
        }
    }

    public void TriggerSpellImpact(Vector3 impactPoint, GameObject sender, float charge = 1, Transform projectile = null)
    {
        //Saves original Element type, might changed based on impactElement.
        ElementType impactVFXType = m_ElementType;
        
        //Channeled Spells should not be scaled by chargetime
        float _ImpactRadius = m_ImpactEffectRadius * (m_SpellType != SpellType.ChanneledRaycast ? charge : 1 );

        foreach (Collider c in Physics.OverlapSphere(impactPoint, _ImpactRadius, m_CollisionLayer + m_PassThroughExplosiveLayer))
        {
            Debug.Log(c.name);
            //Try to apply elemental effect
            if(c.TryGetComponent(out IElementReactant<KeyValuePair<ElementType, ElementState>> Reactant))
            {
                bool affected = false;
                foreach (KeyValuePair<ElementType, ElementState> e in ImpactElementStateIDictionary) 
                {
                    //Try to set Enemy Element state based their current state
                    Reactant.TryAffect(e, out affected); 
                    if (affected == true)
                    {
                        //Hardcoded the last week to save time and avoid any "unwanted behaviours".
                        //Plays a special PoisonVFX if this is a poisonprojectile & enemy is burning
                         if (ElementType == ElementType.Fire && e.Key == ElementType.Poison) 
                            impactVFXType = ElementType.PoisonFire; 
                         //Plays a special FireVFX if this is a fire projectile and enemy is poisoned
                         if (ElementType == ElementType.Poison && e.Key == ElementType.Fire)
                            impactVFXType = ElementType.FirePoison; 
                        break;
                    }
                }
            }
            //Deal Damage
            if (c.TryGetComponent(out IHealth<ElementType> target))
            {
                float distanceValue = DamageByDistance.Evaluate(Vector3.Distance(impactPoint, c.transform.position) / m_ImpactEffectRadius * charge);
                target.TakeDamage(m_Damage * charge * distanceValue, m_ElementType);
            }
            //Tag enemies
            c.GetComponent<ITaggable<GameObject>>()?.Tag(sender);

            //Channeled spells should not destroy things
            if (SpellType != SpellType.ChanneledRaycast) 
            {
                if (c.TryGetComponent(out IDestructible destructible))
                {
                    destructible.Explode(m_ImpactExplosionForce, impactPoint, m_ImpactEffectRadius);
                    continue;
                }
            }
            //Apply Force to parts of Destructible Objects
            if (c.TryGetComponent(out  Rigidbody rb))
                rb.AddExplosionForce(m_ImpactExplosionForce * charge, impactPoint, ImpactRadius, rb.transform.position.y - impactPoint.y, ForceMode.Impulse);   
        }
        //Play sound / VFX
        if (m_SpellType != SpellType.ChanneledRaycast)
        {
            if (AudioManager.instance && m_ImpactSound)
                AudioManager.instance.PlayClipAtPoint(m_ImpactSound, impactPoint, m_TargetAudioMixer);

            if (m_VFXPools)
            {
                GameObject explosion = m_VFXPools.GetVFXPool(VFXType.Impact, impactVFXType).Rent(false);
                explosion.transform.localScale = impactVFXType == m_ElementType ? VisualImpactRadiusScaledByElementType * charge : Vector3.one * 0.8f;
                explosion.transform.position = impactPoint;
                explosion.transform.rotation = projectile ? projectile.transform.rotation : Quaternion.identity;
                explosion.SetActive(true);
            }
        }
    }
}
[System.Serializable]
public class EnumElementStateDictionary : SerializableDictionary<ElementType, ElementState>
{

}

[System.Serializable]
public class ElementStateTypeVFXDictionary : SerializableDictionary<ElementType, ElementType> 
{ 

}


