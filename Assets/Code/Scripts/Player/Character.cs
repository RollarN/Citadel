using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

public abstract class Character : MonoBehaviour, IElementAffectable, IHealth<ElementType>
{



    #region SpellBasedGfx
    [Header("Charging Spell VFX Things")]
    [SerializeField] protected GameObject[] m_SpellBasedMaterialUsers; //Glowing Armpads
    [SerializeField] protected Material m_SpellBasedMaterial;
    [SerializeField] protected float m_MaxEmissionIntensity;
    [SerializeField] protected ParticleSeeker m_LightningPfxParticle;
    
    //Color to emit from armpads
    private Color m_SpellColor = Color.red;
    protected Color SpellColor
    {
        get => m_SpellColor;
        set
        {
            m_SpellColor = value;
            m_SpellBasedMaterial.SetColor("_BaseColor", value);
        }
    }
    #endregion SpellBasedGfx
    [Header("SFX")]
    [SerializeField]protected AudioSource m_SpellAudioSource;
    [SerializeField]private AudioSource m_GeneralAudioSource;
    [SerializeField]private AudioClip[] m_DamageTakenClips;
    [SerializeField]private float m_DamageTakenSFXCooldown;
    private float m_DamageTakenSFXCooldownTime;

    public AudioSource GeneralAudioSource => m_GeneralAudioSource;
    public AudioSource SpellAudioSource => m_SpellAudioSource;
    #region SpellCasting

    protected Vector3 m_CastingPoint => transform.position + Vector3.up* 0.5f;
    protected float m_ChargeTimeAccu = 0;
    protected virtual float ChargeTimeAccu
    {
        get => m_ChargeTimeAccu;
        set
        {
            m_ChargeTimeAccu = value;
            m_SpellBasedMaterial.SetColor("_EmissiveColor", Color.Lerp(Color.black, SpellColor, m_ChargeTimeAccu) * m_ChargeTimeAccu * m_MaxEmissionIntensity);
        }
    }
    protected float m_ChargeTimeScaledByMaxChargeTime => ChargeTimeAccu / m_TimeToReachFullCharge;
    protected virtual float m_TimeToReachFullCharge => 1f;
    protected virtual bool m_CanCharge => CanShoot && !IsCharging;

    protected float m_CastWeight; //Weights rotation between mouse coursor-direction and Movement Direction
    protected abstract float ChargingMovementSpeedMultiplier { get;}

    protected bool IsCharging => m_ChargeCoroutine != null;
    protected IEnumerator m_ChargeCoroutine;
    #endregion SpellCasting
    [Header("Ragdoll And Animations")]
    [SerializeField]protected GameObject[] RagDollJoints; 
    [SerializeField] [Range(0, 1)] private float m_rotationSmoothing = 0.8f;
    [SerializeField] [Range(0, 5)] private float m_CastWeightMultiplierMagnitude = 5f;
    private Animator m_Animator;
    protected Animator Animator => m_Animator;
    private const string k_IsRunning = "IsRunning";
    private const string k_IsCasting = "IsCasting";
    private const string k_FinishCasting = "FinishCasting";
    private const string k_IsShocked = "IsShocked";
    private const string k_YMove = "YMove";

    [Header("ObjectPool")]
    [SerializeField] private SpellVFXPools m_VFXPools = null;

    public ElementState CurrentElement { get; private set; }

    protected float m_StunTime;

    public abstract float StunTime
    { 
      get;
      set;
    }
    #region BoolProperties
    protected bool IsStunned => m_StunTime > Time.time; 
    protected bool CanMove => !IsStunned;
    public bool CanShoot
    {
        get
        { 
            if (!IsStunned)
            {
                return Mathf.Max(m_StunTime, m_CoolDownTimer) <= Time.time; 
            }
            return false;
        }
    }
    #endregion BoolProperties
    #region Movement
    protected float MovementSpeed
    {
        get => m_BaseMovementSpeed * MovementMultipliers * (IsStunned ? 0f : 1f);
    }

    private float m_SlowAmount;

    protected float SlowAmount
    {
        get => m_SlowAmount;
        set => m_SlowAmount = Mathf.Clamp(value, 0, 1);
    }
    protected float MovementMultipliers => 1 - Mathf.Clamp(SlowAmount + ChargingMovementSpeedMultiplier, 0, 1f);

    protected virtual void UpdateAnimationLayersAndRotation()
    {
        float castweighMulti = m_CastWeightMultiplierMagnitude * (IsCharging ? 1 : -1);
        m_CastWeight = Mathf.Clamp(m_CastWeight + Time.deltaTime * castweighMulti, 0, 1);
        Quaternion movementBasedRot = Quaternion.LookRotation(MovementVector.Flattened().magnitude == 0 ? transform.forward : MovementVector.Flattened());
        Vector3 diff = (AimTargetPosition - transform.position).Flattened();
        if (diff != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.Lerp(movementBasedRot, Quaternion.LookRotation(diff), m_CastWeight);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, m_rotationSmoothing);
        }
        Animator.SetLayerWeight(1, m_CastWeight * 2);
        Animator.SetFloat(k_YMove, MovementVector.normalized.magnitude * (MovementSpeed / m_BaseMovementSpeed));
        Animator.SetBool(k_IsRunning, MovementVector.magnitude * MovementSpeed > 0);
    }
    protected abstract Vector3 MovementVector { get; }

    [Header("Stats")]
    [SerializeField] private float m_BaseMovementSpeed = 10;
    #endregion Movement
    protected abstract Vector3 AimTargetPosition { get; }
    public virtual void Awake()
    {
        Hp = m_MaxHealth;
        m_SpellBasedMaterial = Instantiate(m_SpellBasedMaterial);
        foreach (GameObject go in m_SpellBasedMaterialUsers)
        {
            go.GetComponent<MeshRenderer>().material = m_SpellBasedMaterial;
        }
        m_SpellBasedMaterial.EnableKeyword("_EMISSION");
        m_Animator = GetComponent<Animator>();
        SpellColor = Color.red;
    }
    public virtual void Update()
    {
        UpdateAnimationLayersAndRotation();
        UpdateChargeVolume();
        DoElementEffects(CurrentElement.EffectsPerSecond, Time.deltaTime);
    }

    private float m_CoolDownTimer;
    #region SpellCasting
    //Overriden by AI, Called By Player
    protected virtual IEnumerator Charge(SpellData spell)
    {

        Animator.SetBool(k_IsCasting, true);
        //Plays Charging Sound
        if(m_SpellAudioSource && spell.ChargingSound)
        {
            //Sets & plays selected Spell's sound.
            m_SpellAudioSource.clip = spell.ChargingSound;
            m_SpellAudioSource.Play();
        }
        if (spell.ElementType.Equals(ElementType.Lightning))
        {
            yield return new WaitForSeconds(spell.MinimumChargeTime);
            m_LightningPfxParticle.GetComponent<ParticleSystem>().Play();
        }
        while (IsCharging)
        {
            UpdateChargeVariables(spell);
            yield return new WaitForEndOfFrame();
        }
    }
    protected abstract void UpdateChargeVariables(SpellData spell);
    protected void StopCharging(ElementType elementType = ElementType.Neutral)
    {
        if (elementType.Equals(ElementType.Lightning))
        {
            m_LightningPfxParticle.GetComponent<ParticleSystem>().Stop();
        }
        if (m_ChargeCoroutine != null)
        {
            StopCoroutine(m_ChargeCoroutine);
            m_ChargeCoroutine = null;
        }
        ChargeTimeAccu = 0;
        if(Animator)
            Animator.SetBool(k_IsCasting, false);
    }
      
    public void CastSpell(Vector3 targetPoint, SpellData spellData, float charge = 1)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (spellData.SpellType == SpellType.ChanneledRaycast)
        {
            Vector3 firingDir = (targetPoint - transform.position).normalized;
            //Raycast to check if it should cause an impact.
            if (Physics.Raycast(m_CastingPoint, firingDir, out RaycastHit hit, spellData.Distance, spellData.CollisionLayer))
            {
                spellData.TriggerSpellImpact(hit.point, gameObject, charge);
            }
            return;
        }


        Animator.SetTrigger(k_FinishCasting);
        m_CastWeight = Mathf.Max(m_CastWeight, 0.8f);
        Vector3 firingPosition = m_CastingPoint + (targetPoint - transform.position).normalized * Mathf.Max(1, spellData.Radius);

        if (spellData.SpellType == SpellType.TargetAOE)
        {
            spellData.TriggerSpellImpact(targetPoint, gameObject, charge);
        }
        else
        {
            //Explode projectiles fired at point blank
            if (Physics.CheckSphere(firingPosition, spellData.Radius, spellData.CollisionLayer))
            {
                spellData.TriggerSpellImpact(firingPosition, gameObject, charge);
            }
            else
            {
                //Gets projectile from object pool and sets their data.
                GameObject projectile = m_VFXPools.GetVFXPool(VFXType.Projectile, spellData.ElementType).Rent(false);

                projectile.GetComponent<Projectile>().SetProjectileData(spellData, gameObject, charge);
                projectile.transform.position = firingPosition;
                projectile.transform.rotation = Quaternion.LookRotation((targetPoint - transform.position).normalized);

                projectile.SetActive(true);
            }
        }


        m_CoolDownTimer = spellData.FiringCooldown + Time.time;

        StopCharging(spellData.ElementType);
        if (AudioManager.instance && spellData.ShootingSound)
            AudioManager.instance.PlayClipAtPoint(spellData.ShootingSound, firingPosition,
                        this.GetType() == typeof(PlayerController) ? 
                        TargetAudioMixer.PlayerSpell : TargetAudioMixer.EnemySpell);
    }

    //Called in Update(), lerps volume based on whether character is charging or not.
    public void UpdateChargeVolume() => m_SpellAudioSource.volume = m_CastWeight;
    #endregion SpellCasting
    #region HealthAndDeath

    [SerializeField] private float m_MaxHealth = 100;

    private float m_HP;
    protected virtual float Hp
    {
        get => m_HP;

        set
        {
            m_HP = Mathf.Clamp(m_HP = value, 0, m_MaxHealth);

            if (m_HP <= 0)
            {
                Die();
            }
        }
    }
    #region IHealth

    public virtual void TakeDamage(float amount, ElementType elementType = ElementType.Neutral)
    {
        TakeDamage(amount);
    }
    public void TakeDamage(float amount)
    {
        Hp -= amount;
        if(m_DamageTakenSFXCooldownTime < Time.time)
        {
            if(m_GeneralAudioSource && m_DamageTakenClips.Length > 0)
            {
                var clip = m_DamageTakenClips[Random.Range(0, m_DamageTakenClips.Length - 1)];
                m_GeneralAudioSource.PlayOneShot(clip);
                float cooldownMulti = CurrentElement?.elementType == ElementType.Lightning ? 0.3f : 1f;
                m_DamageTakenSFXCooldownTime = m_DamageTakenSFXCooldown * cooldownMulti + Time.time;
            }
        }
    }

    public virtual void RestoreHealth(float amount)
    {
        Hp += amount;
    }

    #endregion IHealth

    protected virtual void Die()
    {
        m_SpellAudioSource.volume = 0;
        Dead = true;
        m_LightningPfxParticle?.GetComponent<ParticleSystem>().Stop();
    }

    protected bool m_Dead;
    protected virtual bool Dead
    {
        get => m_Dead;
        set
        {
            ToggleRagdoll = value;
            this.enabled = !value;


            GetComponent<ElementReactant>().CurrentElementState = null;
            GetComponent<ElementReactant>().enabled = !value;
            m_LightningPfxParticle?.gameObject.SetActive(!value);
        }
    }
    protected bool m_ToggledRagDoll;
    protected bool ToggleRagdoll
    {
        get => m_ToggledRagDoll;
        set
        {
            for (int i = 0; i < RagDollJoints.Length; i++)
            {
                RagDollJoints[i].GetComponent<Rigidbody>().isKinematic = !value;
                RagDollJoints[i].GetComponent<Collider>().enabled = value;
            }
            if (m_Animator) 
                m_Animator.enabled = !value;
            GetComponent<Collider>().enabled = !value;
            GetComponent<ElementReactant>().enabled = !value;
            if (TryGetComponent(out Rigidbody body))
            {
                body.isKinematic = value;
                body.velocity = Vector3.zero;
            }
            m_ToggledRagDoll = value;
        }
    }
    #endregion HealthAndDeath
    #region elementMethods
    public void SetElement(ElementState element)
    {
        if (CurrentElement != null)
        {
            DoElementEffects(CurrentElement.EffectsWhenRemoved);
        }


        if (CurrentElement == null || CurrentElement.elementType != element.elementType)
        {
            m_Animator?.SetBool(k_IsShocked, element.elementType == ElementType.Lightning);
        }

        CurrentElement = element;
        DoElementEffects(element.EffectsWhenApplied);

    }

    public void UpdateElement(ElementState element) => DoElementEffects(element.EffectsPerSecond, Time.deltaTime);

    public void DoElementEffects(EffectEnumFloatDictionary effectDictionary, float multiplier = 1)
    {
        foreach (KeyValuePair<ElementEffect, float> effect in effectDictionary)
        {
            switch (effect.Key)
            {
                case ElementEffect.TakeDamage:
                    TakeDamage(effect.Value * multiplier);
                    break;
                case ElementEffect.UpdateMovementSpeed:
                    SlowAmount = 1 - effect.Value;
                    break;
                case ElementEffect.Stun:
                    m_StunTime = Mathf.Max(m_StunTime, effect.Value + Time.time);
                    break;
                default:
                    break;
            }
        }
    }
    #endregion elementMethods
    public void FinishCasting()
    {
        //Xd
    }
}

