using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyPropChomping : IState<EnemyPropController>
{
    private readonly float m_TimeBetweenAttacks = 0.5f;
    private float m_TimeAtLastAttack = 0f;
    private readonly float m_DamagePerTick = 5f;
    private IHealth<ElementType> m_TargetHealthComponent = null;
    private Collider[] m_OwnerColliders = null;
    private readonly List<int> m_NonTriggerColliderIndexBuffer = new List<int>();
    private float m_CapsuleRadiusBuffer = 0f;
    private Vector3 m_ChestJumpTargetOffset = new Vector3(0, 0.5f, 0);

    public void EnterState(EnemyPropController owner)
    {

        if (owner.TryGetComponent(out Animator animator))
        {
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsEating", true);
        }

        m_NonTriggerColliderIndexBuffer.Clear();

        m_OwnerColliders = owner.GetComponentsInChildren<Collider>();
        for(int i = 0; i < m_OwnerColliders.Length; i++)
        {
            if (!m_OwnerColliders[i].isTrigger)
            {
                m_OwnerColliders[i].isTrigger = true;
                m_NonTriggerColliderIndexBuffer.Add(i);
            }
        }
        m_TimeAtLastAttack = Time.time;

        m_CapsuleRadiusBuffer = owner.ColliderComponent.radius;

        owner.ColliderComponent.radius = Mathf.Max(owner.ColliderComponent.radius, 3f);

        m_TargetHealthComponent = owner.TargetObject.GetComponent<IHealth<ElementType>>();
    }

    public void ExitState(EnemyPropController owner)
    {
        for (int i = 0; i < m_NonTriggerColliderIndexBuffer.Count; i++)
        {
            m_OwnerColliders[m_NonTriggerColliderIndexBuffer[i]].isTrigger = false;
        }
        owner.ColliderComponent.radius = m_CapsuleRadiusBuffer;
    }

    public void UpdateState(EnemyPropController owner)
    {
        if (!owner.TargetObject || m_TargetHealthComponent == null)
        {
            owner.SetState(owner.IdleState);
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            owner.gameObject.SetActive(false);
        }

        owner.transform.position = owner.TargetObject.transform.position + m_ChestJumpTargetOffset;

        if(m_TimeAtLastAttack + m_TimeBetweenAttacks < Time.time)
        {
            m_TimeAtLastAttack += m_TimeBetweenAttacks;
            m_TargetHealthComponent.TakeDamage(m_DamagePerTick, ElementType.Neutral);
        }
    }
}
