using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
public enum MimicAttackPattern
{
    None,
    PassThrough,
    BounceBack,
    SpinAndBounceBack,
    ChestJump,
    SuicideThrow
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class EnemyPropController : MonoBehaviour, IHealth<ElementType>, ISavable
{
    [Header("Detection")]
    [SerializeField] private float m_DetectionRadius = 5f;
    [SerializeField] private LayerMask m_DetectionLayer;
    [SerializeField] private float m_DetectionAnimationTime = 5f;
    [SerializeField] private AudioClip m_DetectionClip;
    [HideInInspector]public Material m_Material;



    [Header("Attacking")]
    [SerializeField] private float m_AttackRange = 5f;
    [SerializeField] private float m_TimeBetweenAttacks = 2f;
    [SerializeField] private float m_AttackAnimationTime = 2f;
    [SerializeField] private float m_Damage = 10f;
    [SerializeField] private MimicAttackPattern m_AttackPattern = default;
    //Test //Johan
    [SerializeField] private float m_SuicideThrowForce = 10f;
    public float m_MaximumEmissionIntensity = 20f;

    [Header("Movement")]
    [SerializeField] private float m_MaxMovementSpeed = 6f;
    [SerializeField] private float m_InitialMovementSpeed = 3f;
    [SerializeField] private float m_MovemenIncreasePerSecond = 1f;
    [SerializeField] private float m_RotationSpeed = 20f;
    [SerializeField] private float m_HoverHeightAbovePlayer = 2f;
    [SerializeField] private float m_ClosestRange = 2f;


    private float m_Health = 1f;

    [Header("Miscellaneous")]
    [SerializeField] private Rigidbody m_RigidBody = null;
    [SerializeField] private CapsuleCollider m_Collider = null;
    [SerializeField] private LayerMask m_GroundLayer;

    private GameObject m_TargetObject = null;
    private float m_TimeSinceLastAttack = 0f;
    private bool m_EnabledSinceStart = false;
    private bool m_SavedSinceStart = false;
    private const string k_NothingLayer = "Nothing";

    public IState<EnemyPropController> IdleState { get; private set; } = new EnemyPropIdle();
    public IState<EnemyPropController> DetectionState { get; private set; } = new EnemyPropDetection();
    public IState<EnemyPropController> FollowState { get; private set; } = new EnemyPropFollow();
    public IState<EnemyPropController> AttackState { get; private set; } = new EnemyPropAttack();
    public IState<EnemyPropController> ChompState { get; private set; } = new EnemyPropChomping();
    private StateMachine<EnemyPropController> m_StateMachine = null;

    public float SuicideThrowForce
    {
        get => m_SuicideThrowForce; 
    }
    public AudioClip DetectionClip
    {
        get => m_DetectionClip;
    }
    public float DetectionRadius
    {
        get => m_DetectionRadius;
    }

    public LayerMask DetectionLayer
    {
        get => m_DetectionLayer;
    }

    public MimicAttackPattern AttackPattern
    {
        get => m_AttackPattern;
        set => m_AttackPattern = value;
    }

    public float HoverHeight
    {
        get => m_HoverHeightAbovePlayer;
    }

    public float MaxMovementSpeed
    {
        get => m_MaxMovementSpeed;
    }

    public LayerMask GroundLayer
    {
        get => m_GroundLayer;
    }

    public float InitialMovementSpeed
    {
        get => m_InitialMovementSpeed;
    }
    
    public float MovementIncreasePerSecond
    {
        get => m_MovemenIncreasePerSecond;
    }
    public float AttackRange
    {
        get => m_AttackRange;
    }
    public float ClosestRange
    {
        get => m_ClosestRange;
    }

    public float TimeBetweenAttacks
    {
        get => m_TimeBetweenAttacks;
    }

    public float AttackAnimationTime
    {
        get => m_AttackAnimationTime;
    }

    public float RotationSpeed
    {
        get => m_RotationSpeed;
    }
    public float TimeAtLastAttack
    {
        get => m_TimeSinceLastAttack;
        set => m_TimeSinceLastAttack = value;
    }
    public float DetectionAnimationTime
    {
        get => m_DetectionAnimationTime;
    }

    public float Damage
    {
        get => m_Damage;
    }

    public Rigidbody RigidBodyComponent
    {
        get => m_RigidBody;
    }

    public CapsuleCollider ColliderComponent
    {
        get => m_Collider;
    }

    public GameObject TargetObject
    {
        get => m_TargetObject;
        set => m_TargetObject = value;
    }
    public bool ObjectActiveOnSave { get; set; }
    private GameObject TargetObjectOnSave { get; set; } = null;
    private IState<EnemyPropController> StateOnSave { get; set; } = null;

    private void Awake()
    {
        m_EnabledSinceStart = true;
        m_DetectionLayer = LayerMask.GetMask("PlayerLayer");
        m_GroundLayer = LayerMask.GetMask("GroundLayer");

        if(AttackPattern == MimicAttackPattern.None)
        {
            enabled = false;
        }
        if (TryGetComponent(out MeshRenderer renderer))
            m_Material = renderer.GetComponent<MeshRenderer>().material;
        else
            m_Material = GetComponentInChildren<MeshRenderer>().material;
        m_Material.EnableKeyword("_EMISSION");

        if (m_RigidBody == null)
        {
            m_RigidBody = GetComponent<Rigidbody>();
        }

        if(m_Collider == null)
        {
            m_Collider = GetComponent<CapsuleCollider>();
        }

        m_StateMachine = new StateMachine<EnemyPropController>(this);
        m_StateMachine.ChangeState(IdleState);
    }
    private void Start()
    {
        //IdleState = new EnemyPropIdle();
        //DetectionState = new EnemyPropDetection();
        //FollowState = new EnemyPropFollow();
        //AttackState = new EnemyPropAttack();
        //ChompState = new EnemyPropChomping();



    }


    void Update()
    {
        gameObject.name = m_StateMachine.currentState.ToString();
        m_StateMachine.UpdateState();

        if (m_Health <= 0)
        {
            m_Health = 0;
            gameObject.SetActive(false);
        }
    }

    public void SetState(IState<EnemyPropController> state)
    {
        m_StateMachine.ChangeState(state);
    }

    private void OnValidate()
    {
        m_AttackRange = Mathf.Clamp(m_AttackRange, m_ClosestRange, float.MaxValue);
        m_TimeBetweenAttacks = Mathf.Clamp(m_TimeBetweenAttacks, m_AttackAnimationTime, float.MaxValue);

        if(m_DetectionLayer == LayerMask.NameToLayer(k_NothingLayer))
        {
            m_DetectionLayer = LayerMask.NameToLayer("PlayerLayer");
        }
    }

    public void TakeDamage(float amount, ElementType elementType = ElementType.Neutral)
    {
        m_Health -= amount;

        if (m_Health <= 0)
        {
            m_Health = 0;
            gameObject.SetActive(false);
        }
    }


    public void RestoreHealth(float amount)
    {
        m_Health += amount;
    }

    public void OnSave(StreamWriter sw, out int addedCount)
    {
        if (!m_EnabledSinceStart)
        {
            addedCount = 0;
            return;
        }
        m_SavedSinceStart = true;
        sw.WriteLine(transform.position.Serialize());
        sw.WriteLine(transform.rotation.Serialize());
        sw.WriteLine(m_TimeSinceLastAttack);
        sw.WriteLine(m_Health);
        sw.WriteLine(gameObject.activeSelf);
        addedCount = 5;

        TargetObjectOnSave = m_TargetObject;
        StateOnSave = m_StateMachine.currentState;
    }

    public void OnLoad(string[] savedData, int startIndex)
    {
        if(!m_EnabledSinceStart || !m_SavedSinceStart)
        {
            return;
        }
        transform.position = savedData[startIndex++].DeserializeToVector3();
        transform.rotation = savedData[startIndex++].DeserializeToQuaternion();
        m_TimeSinceLastAttack = float.Parse(savedData[startIndex++]);
        m_Health = float.Parse(savedData[startIndex++]);
        gameObject.SetActive(bool.Parse(savedData[startIndex++]));

        m_TargetObject = TargetObjectOnSave;

        if(StateOnSave != m_StateMachine.currentState)
        {
            SetState(StateOnSave);
        }
    }
}
