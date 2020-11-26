using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPropFollow : IState<EnemyPropController>
{
    private float m_CurrentMovementSpeed = 0f;
    private float m_EffectiveMovementSpeed = 0f;

    Quaternion targetRotation = Quaternion.identity;
    
    public void EnterState(EnemyPropController owner)
    {
        m_CurrentMovementSpeed = m_EffectiveMovementSpeed = owner.InitialMovementSpeed;
    }

    public void ExitState(EnemyPropController owner)
    {
        
    }

    public void UpdateState(EnemyPropController owner)
    {
        MoveTowardsTarget(owner);

        Quaternion targetRotation = Quaternion.LookRotation((owner.TargetObject.transform.position - owner.transform.position).normalized);
        float angle = Quaternion.Angle(owner.transform.rotation, targetRotation);


        if(Time.time - owner.TimeAtLastAttack > owner.TimeBetweenAttacks
            && (owner.TargetObject.transform.position - owner.transform.position).magnitude < owner.AttackRange 
            && (owner.AttackPattern == MimicAttackPattern.ChestJump || angle < 1f))
        {
            owner.SetState(owner.AttackState);
            return;
        }

        owner.transform.rotation = Quaternion.RotateTowards
                                   (
                                       owner.transform.rotation,
                                       targetRotation,
                                       Mathf.Clamp(angle * Time.deltaTime, owner.RotationSpeed, float.MaxValue)
                                   );
    }


    private void MoveTowardsTarget(EnemyPropController owner)
    {
        owner.transform.position += (owner.TargetObject.transform.position - owner.transform.position).normalized.Flattened() 
                                        * (Time.deltaTime * m_EffectiveMovementSpeed);
        m_CurrentMovementSpeed = Mathf.Clamp(m_CurrentMovementSpeed + owner.MovementIncreasePerSecond * Time.deltaTime, 
                                                owner.InitialMovementSpeed, 
                                                owner.MaxMovementSpeed);

        if((owner.TargetObject.transform.position - owner.transform.position).magnitude < owner.AttackRange)
        {
            m_EffectiveMovementSpeed = m_CurrentMovementSpeed * ((owner.TargetObject.transform.position - owner.transform.position).magnitude - owner.ClosestRange);
        }
        else
        {
            m_EffectiveMovementSpeed = m_CurrentMovementSpeed;
        }
        if(owner.AttackPattern == MimicAttackPattern.ChestJump)
        {
            owner.transform.position = owner.transform.position.Flattened() + BounceFofY(Time.time, owner);
        }
    }

    private void MoveTowardsTargetChest(EnemyPropController owner)
    {
        owner.transform.position += (owner.TargetObject.transform.position - owner.transform.position).normalized * m_CurrentMovementSpeed;
            //Vector3.up * BounceFofY;
    }

    private Vector3 BounceFofY(float t, EnemyPropController owner)
    {
        t %= 1;
        float groundHeight = 0;
        if(Physics.Raycast(owner.transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hitInfo))
        {
            groundHeight = hitInfo.point.y;
        }
        owner.transform.rotation = Quaternion.LookRotation(owner.transform.forward.Flattened().normalized, Vector3.up) 
                                    * Quaternion.AngleAxis(45 * Mathf.Sin(2 * t * Mathf.PI), owner.transform.right);
        
       
        return Vector3.up * ((Mathf.Sin(2 * t * Mathf.PI - Mathf.PI * 0.5f) + 1) * 0.5f + groundHeight);
    }

    //private void BounceAnimation()
}
