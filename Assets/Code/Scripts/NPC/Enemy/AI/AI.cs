using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;

public class AI : Character, ITaggable<GameObject>, ISavable
{
    //Last minute fix

    private int m_strafeValue = 1;
    [Header("Drops")]
    [SerializeField] private GameObject DropPrefab;
    [Range(0f, 100f)]
    [SerializeField] private float m_DropChance = 5f;
    [Header("AI Components")]
    [SerializeField] private SpellData m_SpellData = null;
    [SerializeField] private Transform m_Eyes = null;

    [Header("Agent Settings")]
    [SerializeField] private float m_TurnSpeed = 5f;
    [SerializeField] private float m_Acceleration = 8f;
    [SerializeField] protected Bar healthBar;

    [Header("Detection Settings")]
    [SerializeField] private float m_PlayerDetectionRadius = 10f;
    [SerializeField] private float m_MaximumAttackRange = 10f;
    [SerializeField] private float m_AIAttackRange = 5f;
    [SerializeField] private LayerMask m_PlayerLayer;
    private Vector3 m_DestinationNearPlayer = Vector3.zero;

    [Header("Crowd Control Effects ?")]
    [SerializeField] private LayerMask m_GroundLayer = default;
    [SerializeField] [Range(0f, -10f)] private float m_FallSpeedAfterPushBack = -3f;
    [SerializeField] public float m_FallSpeedDeathTreshhold = 5f;


    private bool m_EnabledSinceStart = false;
    private bool m_SavedSinceStart = false;
    private IState<AI> m_StateOnSave = null;
    private GameObject m_PlayerTargetOnSave = null;

    private Vector3 m_Vector3Infinity = Vector3.one * Mathf.Infinity;
    public float SqrMaxAttackRange => m_MaximumAttackRange * m_MaximumAttackRange;
    public float SqrDetectionRadius => m_PlayerDetectionRadius * m_PlayerDetectionRadius;
    public float SqrAttackRange => m_AIAttackRange * m_AIAttackRange;
    public float PlayerDetectionRadius => m_PlayerDetectionRadius;
    public bool CanSeePlayer { get; set; } = false;
    public bool IsTagged { get; set; } = false;
    public bool IsOnGround { get; set; } = false;
    public Vector3 LastKnownTargetPosition { get; set; } = default;
    

    public LayerMask PlayerLayer
    {
        get => m_PlayerLayer;
        set => m_PlayerLayer = value;
    }
    public override float StunTime
    {
        get => m_StunTime;

        set 
        {
            if (Agent)
            {
                Agent.enabled = false;
            }
            StopCharging(m_SpellData.ElementType);
            m_StunTime = value + Time.time;
        }
    }

    private Vector3 m_RandomDirection = Vector3.zero;
    private float m_AttackTimer = 0f;
    private bool ShowDebugThings = false;

    // AI Components
    protected override Vector3 MovementVector => Agent.desiredVelocity;
    protected override float Hp { set { healthBar.SetValue(value); base.Hp = value; } }
    protected override float ChargingMovementSpeedMultiplier => m_CastWeight * m_SpellData.ChargingMovementSpeedDividend;
    protected override Vector3 AimTargetPosition => PlayerTarget ? PlayerTarget.transform.position : transform.position;
    public NavMeshAgent Agent { get; private set; }
    public StateMachine<AI> StateMachine { get; private set; }
    public Idle IdleState { get; private set; }
    public Follow FollowState { get; private set; }
    public Attack AttackState { get; private set; }
    public AISearch SearchState { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public GameObject PlayerTarget { get; set; }
    public bool ObjectActiveOnSave { get; set; }

    private LineRenderer m_Line;

    public event Action OnTag;

    public override void Awake()
    {
        base.Awake();
        healthBar.SetMaxValue(Hp);
        m_EnabledSinceStart = true;
        Rigidbody = GetComponent<Rigidbody>();
        m_Line = gameObject.AddComponent<LineRenderer>(); 

        AgentSetup();
        switch (m_SpellData.ElementType)
        {
            case ElementType.Fire:
                SpellColor = Color.red;
                break;
            case ElementType.Ice:
                SpellColor = Color.blue;
                break;
            case ElementType.Lightning:
                SpellColor = Color.white;
                break;
            case ElementType.Poison:
                SpellColor = Color.green;
                break;
        }
        m_SpellBasedMaterial.SetColor("_EmissiveColor", SpellColor * m_MaxEmissionIntensity / 2f);
        
        IdleState = new Idle();
        FollowState = new Follow();
        AttackState = new Attack();
        SearchState = new AISearch();
        StateMachine = new StateMachine<AI>(this);

        StateMachine.ChangeState(IdleState);
    }

    public override void Update()
    {
        
        AIOnGround();
         
        Agent.speed = MovementSpeed;
        base.Update();
        if (IsStunned)
        {
            return;
        }
        DebugAgentPath();
        if (PlayerTarget)
        {
            PlayerInLineOfSight(m_Eyes.position, DirectionToPlayer(transform.position, PlayerTarget.transform.position));
        }

        PlayerIsDead();

        if (Agent.enabled)
        {
            StateMachine.UpdateState();
            UpdateLookRotation();
        }
    }

    #region CharacterMethods

    protected override bool Dead
    {
        get => m_Dead;
        set
        {
            base.Dead = value;
            if (Agent)
            {
                Agent.velocity = value ? Vector3.zero : Agent.velocity;
                if(Agent.isOnNavMesh)
                    Agent.isStopped = value;
            }
            if(healthBar)
                healthBar.gameObject.SetActive(!value);

            StopCharging(m_SpellData.ElementType);

            if (value == true)
                DropItem(transform.position);
        }

    }

    protected override IEnumerator Charge(SpellData spell)
    {
        //Resets destination when initiating Charge
        Agent.SetDestination(transform.position);
        if (spell.ElementType == ElementType.Lightning)
        {
            m_LightningPfxParticle.target = PlayerTarget.transform;
        }
        StartCoroutine(base.Charge(spell));
        yield return null;
    }
    //Called in Update() by Coroutine (Character.Charge())
    protected override void UpdateChargeVariables(SpellData spell)
    {
        ChargeTimeAccu += Time.deltaTime;
        if (spell.SpellType.Equals(SpellType.ChanneledRaycast))
        {
            spell.TriggerSpellImpact(m_LightningPfxParticle.target.position, null, Time.deltaTime);
        }

        //Interrupt if player vision is abrupted.
        if (!CanSeePlayer && SqrDistanceToPlayer(transform.position, PlayerTarget.transform.position) > SqrMaxAttackRange)
        {
            StopCharging(spell.ElementType);
        }

        //AI will always cast spells until full charge
        if (ChargeTimeAccu > spell.TimeToReachFullCharge)
        {
            switch (spell.SpellType)
            {
                case SpellType.Projectile:
                case SpellType.TargetAOE:
                    CastSpell(AimTargetPosition, spell);
                    break;
                case SpellType.ChanneledRaycast:
                    if (DistanceToPlayer(transform.position, PlayerTarget.transform.position) > m_MaximumAttackRange)
                        StopCharging(spell.ElementType);
                    break;
            }
        }
    }
    #endregion CharacterMethods

    private void AgentSetup()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.speed = MovementSpeed;
        Agent.acceleration = m_Acceleration;
    }

    private void AIOnGround()
    {
        IsOnGround = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDistance: 2f, m_GroundLayer);

        if (!IsOnGround)
        {
            IncreaseGravity();
        }

        if (IsOnGround && !IsStunned)
        {
            Agent.enabled = true;
            if (Rigidbody.velocity.y >= m_FallSpeedDeathTreshhold)
                Die();
        }
    }

    private void IncreaseGravity()
    {
        Rigidbody.velocity += Vector3.down * m_FallSpeedAfterPushBack;
    }

    public bool SetDestinationNearPlayer(Vector3 playerPosition)
    {
        if (NavMesh.SamplePosition(playerPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            m_DestinationNearPlayer = hit.position;
            return true;
        }

        m_DestinationNearPlayer = m_Vector3Infinity;
        return false;
    }

    public void GoToNewDestination()
    {
        if (m_DestinationNearPlayer == m_Vector3Infinity)
            return;

        Agent.SetDestination(m_DestinationNearPlayer);
        Agent.isStopped = false;
    }

    public void FollowMove()
    {
        if (!PlayerTarget)
            return;

        Vector3 curPos = Agent.transform.position;
        Vector3 playerPos = PlayerTarget.transform.position;
        if (SqrDistanceToPlayer(curPos, playerPos) > SqrAttackRange)
        {
                Agent.SetDestination(playerPos);
        }
    }

    [System.Obsolete("Needs an update")]
    private void MoveBack()
    {
        if (!PlayerTarget)
            return;

        Vector3 curPos = Agent.transform.position;
        Vector3 playerPos = PlayerTarget.transform.position;

        if (SqrDistanceToPlayer(curPos, playerPos) < SqrAttackRange * 0.1f)
        {
            MoveAgent(-DirectionToPlayer(curPos, playerPos), MovementSpeed);
        }
    }

    private void MoveAgent(Vector3 direction, float speed)
    {
        Agent.Move(direction * speed * Time.deltaTime);
    }

    private void UpdateLookRotation()
    {
        if (CanSeePlayer && PlayerTarget)
        {
            Vector3 direction = DirectionToPlayer(transform.position, PlayerTarget.transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), m_TurnSpeed * Time.deltaTime);
        }
        else if (m_RandomDirection != Vector3.zero) // Should be removed in later update
        {
            Vector3 direction = m_RandomDirection;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), m_TurnSpeed * Time.deltaTime);
        }
    }

    #region Calculation functions

    /// <summary>
    /// Returns a Vector3 direction from curPos to playerPos normalized.
    /// </summary>
    /// <param name="curPos">Current position.</param>
    /// <param name="playerPos">Player position.</param>
    /// <returns></returns>
    public Vector3 DirectionToPlayer(Vector3 curPos, Vector3 playerPos)
    {
        return (playerPos - curPos).normalized;
    }

    /// <summary>
    /// Returns a squared length from curPos to playerPos.
    /// </summary>
    /// <param name="curPos">Current position.</param>
    /// <param name="playerPos">Player position.</param>
    /// <returns></returns>
    public float SqrDistanceToPlayer(Vector3 curPos, Vector3 playerPos)
    {
        return (playerPos - curPos).sqrMagnitude;
    }

    /// <summary>
    /// Returns the length from curPos to playerPos.
    /// </summary>
    /// <param name="curPos">Current position.</param>
    /// <param name="playerPos">Player position.</param>
    /// <returns></returns>
    public float DistanceToPlayer(Vector3 curPos, Vector3 playerPos)
    {
        return (playerPos - curPos).magnitude;
    }

    #endregion Calculation functions

    private void PlayerInLineOfSight(Vector3 origin, Vector3 direction)
    {
        float rayDistance = DistanceToPlayer(origin, PlayerTarget.transform.position);
        origin += Vector3.up * 0.05f;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance))
        {
            if(CanSeePlayer = hit.collider.GetComponent<PlayerController>())
            {
                LastKnownTargetPosition = PlayerTarget.transform.position;
            }
        }
    }

    public void AttackPlayer()
    {

        if (m_CanCharge)
        {
            m_ChargeCoroutine = Charge(m_SpellData);
            StartCoroutine(m_ChargeCoroutine);
        }
        //Sets new Destination if current destination is reached.
        else if (Agent.remainingDistance <= Agent.stoppingDistance)
        {

            if (SqrDistanceToPlayer(transform.position, PlayerTarget.transform.position) <= SqrAttackRange)
            {   //Moves Perpendicular to player direction based on StrafeValue
                MovePerpendicularToDirection(DirectionToPlayer(transform.position, PlayerTarget.transform.position), m_strafeValue);
            }
            else
                Agent.SetDestination(GetClosestPointWithDistanceToTarget(PlayerTarget.transform.position, m_AIAttackRange));
        }
    }
    private void MovePerpendicularToDirection(Vector3 dir, float xValue = 1f)
    {
        Agent.SetDestination(transform.position + Vector3.Cross(dir, Vector3.up) * xValue);
    }
    private void SetDestinationPerpendicularToDirection(Vector3 startPos, Vector3 dir, float xValue = 1f) 
    {
        Agent.SetDestination(startPos +
            Vector3.Cross(dir, Vector3.up) * xValue);
    }
    /// <summary>
    /// Returns the closest position that is *distance* units away from targetPos.
    /// </summary>
    /// <param name="targetPos">Target Position.</param>
    /// <param name="distance">DistanceFrom targetPos.</param>
    /// <returns></returns>
    private Vector3 GetClosestPointWithDistanceToTarget(Vector3 targetPos, float distance)
    {
        return targetPos + ((transform.position - targetPos).normalized * distance);
    }
    public void SetState(IState<AI> state)
    {
        StateMachine.ChangeState(state);
    }
   
    private void DropItem()
    {
        float number = UnityEngine.Random.Range(0, 100f);
    }
    [System.Obsolete("Unreliable")]
    private void DropItem(Vector3 spawnPosition)
    {
        WeightedLootTable.S_WeightedLootTable.RandomItem(spawnPosition);
    }

    private void PlayerIsDead()
    {
        if (!PlayerTarget)
            return;

        if (!PlayerTarget.gameObject.activeSelf)
        {
            PlayerTarget = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, PlayerDetectionRadius);
    }

    private void DebugAgentPath()
    {

        if (Input.GetKeyDown(KeyCode.O))
            ShowDebugThings = !ShowDebugThings;
        if (ShowDebugThings)
        {
            if (Agent.hasPath)
            {
                {
                    m_Line.positionCount = Agent.path.corners.Length;
                    m_Line.SetPositions(Agent.path.corners);
                    m_Line.enabled = true;
                }
            }
            else
            {
                m_Line.enabled = false;
            }
        }
        else
        {
            m_Line.enabled = false;
        }
    }

    public void Tag(GameObject sender)
    {
        PlayerTarget = sender;
        OnTag?.Invoke();
        LastKnownTargetPosition = sender.transform.position;
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
        sw.WriteLine(Dead);
        sw.WriteLine(Hp);

        sw.WriteLine(LastKnownTargetPosition.Serialize());
        sw.WriteLine(Agent.destination.Serialize());
        addedCount = 6;

        m_StateOnSave = StateMachine.currentState;
        m_PlayerTargetOnSave = PlayerTarget;
    }

    public void OnLoad(string[] savedData, int startIndex)
    {
        if (!m_EnabledSinceStart || !m_SavedSinceStart)
        {
            return;
        }

        transform.position = savedData[startIndex++].DeserializeToVector3();
        transform.rotation = savedData[startIndex++].DeserializeToQuaternion();
        Dead = bool.Parse(savedData[startIndex++]);
        Hp = float.Parse(savedData[startIndex++]);
        LastKnownTargetPosition = savedData[startIndex++].DeserializeToVector3();

        if(m_StateOnSave != StateMachine.currentState)
        {
            SetState(m_StateOnSave);
        }

        PlayerTarget = m_PlayerTargetOnSave;

        gameObject.SetActive(ObjectActiveOnSave);
        if(Agent.isOnNavMesh && Agent.enabled)
            Agent.destination = savedData[startIndex++].DeserializeToVector3();
    }
}