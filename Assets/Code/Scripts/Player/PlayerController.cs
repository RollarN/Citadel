using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.IO;

[RequireComponent(typeof(CharacterController))] 

public class PlayerController : Character, ISavable
{
    private bool m_INVINCIBLETEST = false;
    [Header("Out Of Mana CD SFX")]
    [SerializeField] private float m_OomSfxCd = 0.3f;
    private float m_OomSfxCdTime;
    [SerializeField]private AudioClip m_OomSfxClip;
    [Header("References")]
    [SerializeField] private float m_FallSpeed;
    [SerializeField] private LayerMask m_WalkableLayers = default;
    [SerializeField] private LayerMask m_PickupLayerMask = default;
    [SerializeField] private LayerMask m_BlockingLayer = default;
    [SerializeField] private GameObject m_AimingCircle = null;
    [SerializeField] private Transform m_MeshTransform = null;
    [SerializeField] private HUDManager m_HudManager = null;
    [SerializeField] private UpgradeHUDManager m_UpgradeHudManager = null;
    [SerializeField] private AudioClip m_HealAudioClip = null;
    [SerializeField] private SaveManager m_SaveManager = null;
    [SerializeField] private LevelToggleManager m_LevelToggleManager = null;
    private const string k_ChargeInput = "Fire1";

    protected override float Hp
    {
        set
        {
            base.Hp = value;
            m_HudManager.HealthImage.fillAmount = Hp / 100;
        }
    }
    protected override bool Dead
    {

        set
        {
            base.Dead = value;
            m_CharacterController.enabled = !value;
        }
    }
    private CharacterController m_CharacterController;
    private Vector3 m_AimtargetPosition;
    private Camera m_MainCamera;


    #region MovementAndControls
    protected override float ChargingMovementSpeedMultiplier => m_CastWeight * SpellDataDictionary[CurrentSpellElement].ChargingMovementSpeedDividend;
    private Vector3 m_MovementVector;
    protected override Vector3 MovementVector => m_MovementVector;
    protected override Vector3 AimTargetPosition => m_AimtargetPosition;
    #endregion MoveMentAndControls
    public override float StunTime { 
        get => m_StunTime;
        set 
        { 
        StunTime = value + Time.time;
        StopCharging(CurrentSpellElement);
        } 
    }
    #region Spells
    private Image m_ChargeBar;
    protected override bool m_CanCharge => CanShoot && !IsCharging && 
        m_CurrentAmmo > SpellDataDictionary[CurrentSpellElement].ChargeCurve.Evaluate(m_MinimumChargeTime / m_TimeToReachFullCharge) * m_ResourceCostPerShot;
    public ElementTypeSpellDataDictionary SpellDataDictionary;

    private ElementType m_CurrentSpellType = ElementType.Fire;
    private ElementType CurrentSpellElement {
        get => m_CurrentSpellType;
        set {
            m_CurrentSpellType = value;
            m_HudManager.HighlightResourceImage(value);
            switch (m_CurrentSpellType)
            {
                case ElementType.Fire:
                    SpellColor = Color.red;
                    break;
                case ElementType.Ice:
                    SpellColor = Color.blue;
                    break;
                case ElementType.Poison:
                    SpellColor = Color.green;
                    break;
                case ElementType.Lightning:
                    SpellColor = Color.white;
                    break;
            }
        }
    }
    protected override float ChargeTimeAccu
    {
        get => m_ChargeTimeAccu;
        set
        {
            m_ChargeTimeAccu = value;
            m_SpellBasedMaterial.SetColor("_EmissiveColor", Color.Lerp(Color.black, SpellColor, m_ChargeValue) * m_ChargeTimeAccu * m_MaxEmissionIntensity);
            m_ChargeBar.fillAmount = m_ChargeTimeScaledByMaxChargeTime;
        }
    }
    protected override float m_TimeToReachFullCharge => SpellDataDictionary[CurrentSpellElement].TimeToReachFullCharge;
    private float m_MinimumChargeTime => SpellDataDictionary[CurrentSpellElement].MinimumChargeTime;

    private float m_ChargeValue { get => SpellDataDictionary[CurrentSpellElement].ChargeCurve.Evaluate(m_ChargeTimeScaledByMaxChargeTime); }

    private float m_AmmoCostAccu;
    private float m_CurrentAmmo
    {
        get => SpellDataDictionary[CurrentSpellElement].CurrentChargePercent;
        set => SpellDataDictionary[CurrentSpellElement].CurrentChargePercent = value;
    }
    private float m_ResourceCostPerShot { get => SpellDataDictionary[CurrentSpellElement].ResourceCostPerShot; }

    #endregion Spells

    #region PickupThatShouldBeInHUDManagerLol
    private SpellUpgradePickup m_PickUpSelected;
    public SpellUpgradePickup PickUpSelected
    {
        get => m_PickUpSelected;
        set
        {
            bool wasNull = m_PickUpSelected == null;
            m_PickUpSelected = value;
            if (m_PickUpSelected != null && wasNull)
            {
                m_UpgradeHudManager.TogglePickUpPanel(true);
            }
            if (m_PickUpSelected == null && !wasNull)
            {
                m_UpgradeHudManager.TogglePickUpPanel(false);
            }
        }
    }
    private Collider[] m_PickupColliderBuffer = new Collider[1];

    #endregion PickupThatShouldBeInHUDManagerLol
    
    #region DashSpell
    [Header("Dash/Blink")]
    [SerializeField] private float m_DashRange = 0f;
    [SerializeField] private float m_DashCooldown = 2;
    [SerializeField] private float m_DashDuration = 0.2f;
    [SerializeField] private AnimationCurve m_ChargeRecoveryCurve = null;
    [SerializeField] private AudioClip m_DashSound = null;
    [SerializeField] private ParticleSystem m_DashPartricleSystem = null;
    private float m_DashCoolDownTime;
    private float m_DashTime;
    private bool m_Dashing => m_DashTime > Time.time;
    private bool m_CanDash => m_DashCoolDownTime < Time.time;

    public bool ObjectActiveOnSave { get; set; }

    public IEnumerator Dash()
    {
        StopCharging(CurrentSpellElement);
        m_CastWeight = 0;
        m_DashPartricleSystem.Play();
        m_DashTime = m_DashDuration + Time.time;
        m_DashCoolDownTime = m_DashCooldown + Time.time;
        m_MeshTransform.gameObject.SetActive(false);
        m_CharacterController.detectCollisions = false;
        if (m_DashSound)
            AudioManager.instance?.PlayClipAtPoint(m_DashSound, transform.position, TargetAudioMixer.PlayerDashSpell, 0f);
        while (m_Dashing)
        {
            Vector3 p1 = transform.position + m_CharacterController.center + Vector3.up * (-m_CharacterController.height * 0.5F);
            Vector3 p2 = p1 + Vector3.up * m_CharacterController.height;
            //Move to any eventual collision
            if  (Physics.CapsuleCast(p1, p2, m_CharacterController.radius, transform.forward, out RaycastHit hit, m_DashRange / (m_DashDuration / Time.deltaTime), m_WalkableLayers)) {
                m_CharacterController.Move(hit.point - transform.position);
            }
            //If nothing blocks, move in players forward direction
            else
                m_CharacterController.Move(transform.forward * (m_DashRange / (m_DashDuration / Time.deltaTime)));
            yield return new WaitForEndOfFrame();
        }
        m_CharacterController.detectCollisions = true;
        m_MeshTransform.gameObject.SetActive(true);
    }
    #endregion DashSpell
    #region PushSpell
    [Header("PushBackSpell Balance")]
    [SerializeField] private float m_PushBackMagnitude = 120;
    [SerializeField] private float m_PushBackYAxis = 0.7f;
    [SerializeField] private float m_PushBackCoolDown = 2;
    [SerializeField] private float m_PushbackStunTime = 0.3f;
    private float m_PushBackCooldownTime = 1.5f;
    [SerializeField] private float m_PushBackRadius;
    [SerializeField] private ParticleSystem m_PushBackPfx;

    [Header("PushBackSpell Boring things")]

    [SerializeField] private AudioClip m_PushBackSound;
    [SerializeField] private float m_RaycastSweepsHeight = 3;
    [SerializeField] private float m_RaycastSweepsWidth = 3;
    [SerializeField] private LayerMask m_PushBackTargetLayers;
    [SerializeField] private LayerMask m_PushBackBlockingLayer;
    public void PushBackSpell()
    {   //Uses sweeping raycasts,
        if (m_PushBackCooldownTime <= Time.time)
        {

            m_PushBackCooldownTime = m_PushBackCoolDown + Time.time;
            m_PushBackPfx?.Play();
                Vector3 targetPosition = new Vector3(m_AimtargetPosition.x, transform.position.y, m_AimtargetPosition.z);
                
                if(m_PushBackSound)
                    AudioManager.instance?.PlayClipAtPoint(m_PushBackSound, transform.position, TargetAudioMixer.PlayerPushBack, 0);
                
                foreach (Collider c in Physics.OverlapSphere(transform.position, m_PushBackRadius,  m_PushBackTargetLayers))
                {

                    if (!Physics.Linecast(transform.position, c.transform.position, m_PushBackBlockingLayer))
                    {
                        if (c.TryGetComponent(out AI aiComponent))
                            aiComponent.StunTime = m_PushbackStunTime;
                        if (c.TryGetComponent(out Rigidbody body))
                            body.AddExplosionForce(m_PushBackMagnitude , transform.position, m_PushBackRadius, m_PushBackYAxis, ForceMode.Impulse);
                    }
                }
            
        }
    }
    #endregion tempPushSpell
    #region CharacterHealth
    public override void TakeDamage(float amount, ElementType elementType = ElementType.Neutral)
    {
        if(m_Dashing)
            return;
        base.TakeDamage(amount, elementType);
    }
    protected override void Die()
    {
        m_CharacterController.Move(Vector3.zero);
        base.Die();
        Invoke("Reload", 2f);
    }
    #endregion CharacterHealth

    public override void Awake()
    {
        base.Awake();

        Hp += 100f;
        CurrentSpellElement = ElementType.Fire;

        #region SettingUpReferences

        m_AimingCircle = Instantiate(m_AimingCircle);
        m_AimingCircle.transform.localScale = new Vector3(SpellDataDictionary[ElementType.Ice].ImpactRadius, 0.05f, SpellDataDictionary[ElementType.Ice].ImpactRadius);
        m_AimingCircle.SetActive(false);

        m_LightningPfxParticle.target = m_AimingCircle.transform;



        m_CharacterController = GetComponent<CharacterController>();

        m_MainCamera = Camera.main;
        m_MainCamera.GetComponent<CameraMovement>().m_Target = transform;
        m_MainCamera.transform.parent = null;
        m_MainCamera.GetComponent<CameraMovement>().m_Offset = m_MainCamera.transform.position - transform.position;
        m_SpellAudioSource = GetComponent<AudioSource>();
        /*if(hudManager == null)
            hudManager = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDManager>();*/

        //Move to Hudmanager when possible
        #endregion SettingUpReferences
        #region setUpSpellDataDictionaries
        //Setting Up SpellData
        ElementTypeSpellDataDictionary spellDataDictionaryTemp = new ElementTypeSpellDataDictionary();
        foreach (KeyValuePair<ElementType, PlayerSpellData> pair in SpellDataDictionary)
        {
            spellDataDictionaryTemp.Add(pair.Key, Instantiate(pair.Value));
        }
        SpellDataDictionary = spellDataDictionaryTemp;

        Dictionary<ElementState, ElementState> OriginalCloneDictionary = new Dictionary<ElementState, ElementState>();
        foreach (PlayerSpellData ppd in SpellDataDictionary.Values)
        {
            EnumElementStateDictionary tempImpactElementState = new EnumElementStateDictionary();

            foreach (KeyValuePair<ElementType, ElementState> pair in ppd.ImpactElementState)
            {
                if (!OriginalCloneDictionary.ContainsKey(pair.Value))
                {
                    OriginalCloneDictionary.Add(pair.Value, Instantiate(pair.Value));
                }
                tempImpactElementState.Add(pair.Key, OriginalCloneDictionary[pair.Value]);
            }
            ppd.ImpactElementState = tempImpactElementState;
            //}
            //if(hudManager && m_ChargeBar)
            //    m_ChargeBar = Instantiate(m_ChargeBar, hudManager.transform);
            m_ChargeBar = m_HudManager.ChargeImage;
            #endregion setUpSpellDataDictionaries
        }
    }
    public override void Update()
    {
        base.Update();
        foreach (PlayerSpellData ppd in SpellDataDictionary.Values)
        {
            if (ppd != SpellDataDictionary[CurrentSpellElement] || m_ChargeTimeAccu == 0)
                ppd.Recharge();
            m_HudManager.GetResourceImage(ppd.ElementType).fillAmount = ppd.CurrentChargePercent / 100f;
        }

        m_HudManager.DashImage.fillAmount = m_DashCooldown - Mathf.Clamp(m_DashCoolDownTime - Time.time, 0, m_DashCooldown);
        m_HudManager.PushBackImage.fillAmount = m_PushBackCoolDown - Mathf.Clamp(m_PushBackCooldownTime - Time.time, 0, m_PushBackCoolDown);

        if (Input.GetKeyDown(KeyCode.Space))
            if (m_CanDash)
                StartCoroutine(Dash());
        if (m_Dashing)
            return;

        UpdateAim();
        UpdateChargeInput();
        if (Input.GetKeyDown(KeyCode.Mouse1))
            PushBackSpell();

        if (Physics.OverlapSphereNonAlloc(transform.position, 2f, m_PickupColliderBuffer, m_PickupLayerMask) > 0)
        {
            SpellUpgradePickup spellUpgradePickup = m_PickupColliderBuffer[0].GetComponent<SpellUpgradePickup>();
            if (spellUpgradePickup.GetSpellUpgrades(this, out int UpgradeCount, out SpellUpgradeData[] UpgradeArray))
            {
                PickUpSelected = spellUpgradePickup;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    PickUpSelected.gameObject.layer = LayerMask.GetMask("Default");
                    m_UpgradeHudManager.EnableUpgradePanels(UpgradeArray);
                }
            }
        }
        else PickUpSelected = null;

        UpdateSelectedSpell();
        if (Input.GetKeyDown(KeyCode.O))
            m_INVINCIBLETEST = !m_INVINCIBLETEST;
        UpdateMovement();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(m_AimtargetPosition, 1f);
    }
    
    #region MovementMethods
    private void UpdateMovement()
    {
        if (m_Dashing)
            return;
        Vector3 inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
        if (inputVector.sqrMagnitude > 1)
            inputVector = inputVector.normalized;
        
        Vector3 movementVector = new Vector3(inputVector.x * MovementSpeed * Time.deltaTime, 0, inputVector.y * MovementSpeed * Time.deltaTime);
        movementVector = Quaternion.Euler(0, m_MainCamera.transform.eulerAngles.y, 0) * movementVector;


        m_MovementVector = movementVector;
        /* if(movementVector.magnitude > 0)
             transform.rotation = Quaternion.LookRotation((movementVector).normalized); */
        //Kommer cleana upp senare
        if (!Physics.Raycast(transform.position, Vector3.down, 1.1f, m_WalkableLayers))
        {
            movementVector += Vector3.down * m_FallSpeed;
        }
        m_CharacterController.Move(movementVector);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.transform.GetComponent<Rigidbody>();
        if (!body)
            return;

        body.AddForce(hit.moveDirection.Flattened() * 10f);

    }
    #endregion MovementMethods

    #region SpellCastingMethods
    private void RecoverLostAmmo(PlayerSpellData spellData)
    {
        if (spellData.SpellType != SpellType.ChanneledRaycast)
            spellData.CurrentChargePercent += m_AmmoCostAccu * m_ResourceCostPerShot;
    }
    private void UpdateSelectedSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RecoverLostAmmo(SpellDataDictionary[CurrentSpellElement]);
            StopCharging(CurrentSpellElement);
            CurrentSpellElement = ElementType.Poison;
            m_AimingCircle.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RecoverLostAmmo(SpellDataDictionary[CurrentSpellElement]);
            StopCharging(CurrentSpellElement);
            CurrentSpellElement = ElementType.Fire;
            m_AimingCircle.SetActive(false);

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RecoverLostAmmo(SpellDataDictionary[CurrentSpellElement]);
            StopCharging(CurrentSpellElement);
            CurrentSpellElement = ElementType.Ice;
            m_AimingCircle.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            RecoverLostAmmo(SpellDataDictionary[CurrentSpellElement]);
            StopCharging(CurrentSpellElement);
            CurrentSpellElement = ElementType.Lightning;
            m_AimingCircle.SetActive(false);
        }
    }

    //Updates AimTargetPosition based on spell type.
    private void UpdateAim()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
        switch (SpellDataDictionary[CurrentSpellElement].SpellType)
        {
            case SpellType.Projectile:
                Vector3 playerScreenPos = m_MainCamera.WorldToScreenPoint(transform.position);

                float cameraTiltFactor = Mathf.Abs(Mathf.Cos(m_MainCamera.transform.eulerAngles.x * Mathf.Deg2Rad));
                float cameraRotationFactor = m_MainCamera.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

                float angle = Mathf.Atan2((Input.mousePosition.y - playerScreenPos.y) / cameraTiltFactor, (Input.mousePosition.x - playerScreenPos.x))
                                - cameraRotationFactor;

                m_AimtargetPosition = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                break;

            case SpellType.ChanneledRaycast:
                Plane RayCastPlane = new Plane(Vector3.up, transform.position);
                if (RayCastPlane.Raycast(ray, out float enter))
                {
                    Ray ray1 = new Ray(m_CastingPoint, (ray.GetPoint(enter) - m_CastingPoint).normalized.Flattened());
                    float distance = Mathf.Min(SpellDataDictionary[CurrentSpellElement].Distance, (m_CastingPoint - ray.GetPoint(enter)).magnitude);
                    if (Physics.Raycast(ray1, out RaycastHit hit1, distance, m_BlockingLayer))
                    {
                        m_AimtargetPosition = hit1.point;
                        m_AimingCircle.transform.position = m_AimtargetPosition;
                        return;
                    }
                    else
                        m_AimtargetPosition = ray1.GetPoint(distance);
                    m_AimingCircle.transform.position = m_AimtargetPosition;
                }
                break;

            case SpellType.TargetAOE:        
                Plane TargetAOEplane = new Plane(Vector3.up, transform.position + Vector3.down * 0.95f);
                if (TargetAOEplane.Raycast(ray, out float TargetAOEEnter))
                {
                    Ray ray1 = new Ray(transform.position + Vector3.down * 0.95f, (ray.GetPoint(TargetAOEEnter) - transform.position + Vector3.down * 0.95f).normalized.Flattened());
                    float distance = Mathf.Min(SpellDataDictionary[CurrentSpellElement].Distance, (m_CastingPoint - ray.GetPoint(TargetAOEEnter)).magnitude);
                    m_AimtargetPosition = ray1.GetPoint(distance);
                    m_AimingCircle.transform.position = m_AimtargetPosition;
                }
                break; 
        }

    }
    //Start charging if possible
    private void UpdateChargeInput()
    {
        if (Input.GetButtonDown(k_ChargeInput))
        {   
            if (m_CanCharge)
            {
                m_ChargeCoroutine = Charge(SpellDataDictionary[CurrentSpellElement]);
                StartCoroutine(m_ChargeCoroutine);
            }
            else if (!IsCharging)
            {
                //Plays a sound  while trying to charge to early
                if (m_OomSfxCdTime < Time.time && m_OomSfxClip)
                {
                    AudioManager.instance?.PlayClipAtPoint(m_OomSfxClip, transform.position, TargetAudioMixer.PlayerSpell, 0f);
                    m_OomSfxCdTime = m_OomSfxCd + Time.time;
                }
            }
        }
    }

    //Called by a Coroutine charging shots.
    protected override void UpdateChargeVariables(SpellData spell)
    {
        if (spell.SpellType.Equals(SpellType.ChanneledRaycast))
        { 
            //Saves time spent charging
            ChargeTimeAccu += Mathf.Clamp(Time.deltaTime, 0, m_CurrentAmmo * Time.deltaTime);
            m_CurrentAmmo -= m_ResourceCostPerShot * Time.deltaTime;
            //Cast every frame while charging.
            CastSpell(m_AimtargetPosition, spell, Time.deltaTime);
            if (!Input.GetButton(k_ChargeInput) || m_CurrentAmmo == 0)
            {
                StopCharging(spell.ElementType);
            }
        }
        else
        {
            //Charges shot and reduces Ammo Accordingly, 
            ChargeTimeAccu += Mathf.Clamp(Time.deltaTime, 0, m_CurrentAmmo * Time.deltaTime);
            m_CurrentAmmo -= (m_ChargeValue - m_AmmoCostAccu) * m_ResourceCostPerShot;
            //Saves ammo used incase player interrupts shot
            m_AmmoCostAccu += (m_ChargeValue - m_AmmoCostAccu);

            //Check LMB input
            if (!Input.GetButton(k_ChargeInput))
            {
                if (ChargeTimeAccu > m_MinimumChargeTime)
                {
                    CastSpell(m_AimtargetPosition, SpellDataDictionary[CurrentSpellElement], m_ChargeValue);
                    m_AmmoCostAccu = 0;
                }
            }
        }
    }
    #endregion SpellCastingMethods
    #region ISavable
    public void OnSave(StreamWriter sw, out int addedCount)
    {
        sw.WriteLine(transform.position.Serialize());
        sw.WriteLine(transform.rotation.Serialize());
        sw.WriteLine(Dead);
        //sw.WriteLine(m_RigidBody.velocity.Serialize());
        addedCount = 3;
    }

    private void Reload()
    {
        m_SaveManager.LoadGame();
    }
    public void OnLoad(string[] savedData, int startIndex)
    {
        m_LevelToggleManager.EnableAllLevels();
        m_CharacterController.enabled = false;
        transform.position = savedData[startIndex++].DeserializeToVector3();
        transform.rotation = savedData[startIndex++].DeserializeToQuaternion();
        m_CharacterController.enabled = true;
        Hp = 100f;
        Dead = bool.Parse(savedData[startIndex++]);
        gameObject.SetActive(ObjectActiveOnSave);
    }
    #endregion ISavable
}
[System.Serializable]
public class ElementTypeSpellDataDictionary : SerializableDictionary<ElementType, PlayerSpellData> { }
[System.Serializable]
public class ElementTypeFloatDictionary : SerializableDictionary<ElementType, float> { }