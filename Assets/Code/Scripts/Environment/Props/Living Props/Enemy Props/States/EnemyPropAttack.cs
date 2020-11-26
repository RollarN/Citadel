using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPropAttack : IState<EnemyPropController>
{
    private Vector3 m_TargetPosition = Vector3.zero;
    private Vector3 m_OriginPosition = Vector3.zero;
    private Collider[] m_ColliderBuffer = new Collider[1];
    private bool m_Attacking = false;
    private bool m_DealtDamage = false;
    private MimicAttackPattern m_AttackPatternBuffer = MimicAttackPattern.None;
    private Vector3 m_ChestJumpTargetOffset = new Vector3(0, 0.5f, 0);
    private bool m_Thrown = false;

    public delegate void AttackPatternFunction(EnemyPropController owner);



    public void EnterState(EnemyPropController owner)
    {
        m_TargetPosition = owner.TargetObject.transform.position
                            + (owner.TargetObject.transform.position - owner.transform.position).Flattened();

        m_OriginPosition = owner.transform.position;

        owner.ColliderComponent.isTrigger = true;
        m_DealtDamage = false;
        if (owner.TryGetComponent(out Animator animator))
        {
            animator.SetBool("IsAttacking", true);
            animator.SetBool("IsEating", false);
        }
    }

    public void ExitState(EnemyPropController owner)
    {
        owner.ColliderComponent.isTrigger = false;
    }

    public void UpdateState(EnemyPropController owner)
    {
        if (m_Thrown)
            return;


        if (m_Attacking)
        {

            float step = (Time.time - owner.TimeAtLastAttack) / owner.AttackAnimationTime;

            switch (owner.AttackPattern)
            {
                case MimicAttackPattern.PassThrough:
                    owner.transform.position = PassThroughAnimation(m_OriginPosition, m_TargetPosition, step);
                    RotateToOwner(owner);
                    break;
                case MimicAttackPattern.BounceBack:
                    owner.transform.position = BounceBackAnimation(m_OriginPosition, m_TargetPosition, step, owner);
                    break;
                case MimicAttackPattern.SpinAndBounceBack:
                    owner.transform.position = SpinAndBounceAnimation(m_OriginPosition, m_TargetPosition, step, owner);
                    break;
                case MimicAttackPattern.ChestJump:
                    owner.transform.position = ChestChompAnimation(step, owner);
                    break;
                case MimicAttackPattern.SuicideThrow:
                    m_Thrown = true;
                    owner.RigidBodyComponent.isKinematic = false;
                    owner.RigidBodyComponent.AddForce((m_TargetPosition - owner.transform.position).normalized * owner.SuicideThrowForce, ForceMode.VelocityChange);
                    owner.enabled = false;
                    break;
            }

            if (!m_DealtDamage)
            {
                TryHitTarget(owner);
            }
            else
            {
                if(owner.AttackPattern == MimicAttackPattern.ChestJump)
                {
                    owner.transform.position = owner.TargetObject.transform.position + m_ChestJumpTargetOffset;
                    owner.SetState(owner.ChompState);
                    return;
                }
            }
            if (Time.time - owner.TimeAtLastAttack > owner.AttackAnimationTime)
            {
                m_Attacking = false;
                if (m_AttackPatternBuffer != MimicAttackPattern.None)
                {
                    owner.AttackPattern = m_AttackPatternBuffer;
                    m_AttackPatternBuffer = MimicAttackPattern.None;
                }
                owner.SetState(owner.FollowState);
            }
        }
        else if (Time.time - owner.TimeAtLastAttack > owner.TimeBetweenAttacks)
        {
            m_Attacking = true;
            owner.TimeAtLastAttack = Time.time;
        }
        else
        {
            RotateToOwner(owner);
        }
    }

    private Vector3 PassThroughAnimation(Vector3 from, Vector3 to, float t)
    {
        return new Vector3(PassThroughFofXZ(from.x, to.x, t), PassThroughFofY(from.y, to.y, t), PassThroughFofXZ(from.z, to.z, t));
    }

    private float PassThroughFofXZ(float from, float to, float t)
    {
        t = Mathf.Clamp01(t);
        t = -2.0F * t * t * t + 3.0F * t * t;
        return from + (to - from) * t;
    }

    private float PassThroughFofY(float from, float to, float t)
    {
        t = Mathf.Clamp01(t);
        t = Mathf.Sin(t * Mathf.PI) * 0.8f;

        return from + (to - from) * t;
    }

    private Vector3 BounceBackAnimation(Vector3 from, Vector3 to, float t, EnemyPropController owner)
    {
        return new Vector3(BounceBackFofXZ(from.x, to.x, t), BounceBackFofY(from.y, to.y, t, owner), BounceBackFofXZ(from.z, to.z, t));
    }

    private float BounceBackFofY(float from, float to, float t, EnemyPropController owner)
    {
        t = Mathf.Clamp01(t);
        if(t < 0.5f)
        {
            //t = 2 * Mathf.Sqrt(t - t * t);
            t = (Mathf.Pow(2f * t - 1f, 3) + 1f) * 0.8f;
            RotateToOwner(owner);
        }
        else
        {
            if (!m_DealtDamage)
            {
                owner.AttackPattern = MimicAttackPattern.PassThrough;
                m_AttackPatternBuffer = MimicAttackPattern.BounceBack;
                return PassThroughFofY(from, to, t);
            }

            t = -Mathf.Pow(2f * t - 2f, 3) * 0.8f;
            owner.transform.rotation = Quaternion.RotateTowards(
                                          owner.transform.rotation,
                                          Quaternion.LookRotation(owner.transform.up, -owner.transform.forward), 720f * Time.deltaTime);    
        }

        

        return from + (to - from) * t;
        //if (t < 0.5f)
        //{
        //    t *= 2;
        //    t = -Mathf.Pow(t, Mathf.Pow(t, Mathf.Pow(t, t))) + t + 1;
        //    RotateToOwner(owner);
        //    return from + (to - from) * t;
        //}
        //else
        //{
        //    if (!m_DealtDamage)
        //    {
        //        owner.AttackPattern = MimicAttackPattern.PassThrough;
        //        m_AttackPatternBuffer = MimicAttackPattern.BounceBack;
        //    }
        //    t -= 0.5f;
        //    t = Mathf.Pow(t, Mathf.Pow(t, Mathf.Pow(t, t))) - t;
        //    owner.transform.rotation = Quaternion.RotateTowards(
        //                                    owner.transform.rotation,
        //                                    Quaternion.LookRotation(owner.transform.up, -owner.transform.forward), 720f * Time.deltaTime);
        //    return from + (to - from) * t;
        //}
    }

    private float BounceBackFofXZ(float from, float to, float t)
    {
        t = Mathf.Clamp01(t);
        t = t < 0.5f ? t : 1 - t;
        t = -2.0F * t * t * t + 3.0F * t * t;
        return from + (to - from) * t;
    }


    private Vector3 SpinAndBounceAnimation(Vector3 from, Vector3 to, float t, EnemyPropController owner)
    {
        return new Vector3(SpinAndBounceFofXZ(from.x, to.x, t), SpinAndBounceFofY(from.y, to.y, t, owner), SpinAndBounceFofXZ(from.z, to.z, t));
    }

    private float SpinAndBounceFofY(float from, float to, float t, EnemyPropController owner)
    {
        t = (t < 0.5f) ? (2 * t) : (-2 * t + 2) * 0.8f;
        owner.transform.rotation *= Quaternion.AngleAxis(owner.RotationSpeed, owner.transform.up);
        return from + (to - from) * t;
    }

    private float SpinAndBounceFofXZ(float from, float to, float t)
    {
        t = Mathf.Clamp01(t);
        t = t < 0.5f ? t : 1 - t;
        t = -2.0F * t * t * t + 3.0F * t * t;
        return from + (to - from) * t;
    }


    private Vector3 ChestChompAnimation(float t, EnemyPropController owner)
    {
        t = Mathf.Clamp01(t);
        return owner.transform.position + (owner.TargetObject.transform.position - (owner.transform.position + m_ChestJumpTargetOffset)) * t;
    }

    private void TryHitTarget(EnemyPropController owner)
    {
        if (Physics.OverlapSphereNonAlloc(owner.transform.position, owner.ColliderComponent.bounds.extents.magnitude, m_ColliderBuffer, owner.DetectionLayer) > 0)
        {
            if (m_ColliderBuffer[0].gameObject.TryGetComponent(out IHealth<ElementType> healthComponent))
            {
                healthComponent.TakeDamage(owner.Damage, ElementType.Neutral);
                m_DealtDamage = true;
            }
        }
    }

    private void RotateToOwner(EnemyPropController owner)
    {
        owner.transform.rotation = Quaternion.LookRotation(owner.TargetObject.transform.position - owner.transform.position);
    }
}
