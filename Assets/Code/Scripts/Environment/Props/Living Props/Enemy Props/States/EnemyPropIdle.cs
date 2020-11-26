using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPropIdle : IState<EnemyPropController>
{
    private readonly Collider[] m_ColliderBuffer = new Collider[1];

    public void EnterState(EnemyPropController owner)
    {
        owner.RigidBodyComponent.useGravity = true;
        owner.RigidBodyComponent.isKinematic = false;
        owner.m_Material.SetColor("_EmissiveColor", Color.white * 0);
        if (owner.TryGetComponent(out Animator animator))
        {
            animator.SetBool("IsEating", false);
            animator.SetBool("IsAttacking", false);
        }
    }

    public void ExitState(EnemyPropController owner)
    {
        
    }

    public void UpdateState(EnemyPropController owner)
    {
        if (DetectPlayerBySphere(owner))
        {
            owner.SetState(owner.DetectionState);
        }
    }

    private bool DetectPlayer(EnemyPropController owner)
    {
        return DetectPlayerBySphere(owner);
    }

    private bool DetectPlayerBySphere(EnemyPropController owner)
    {
        int detectedObjects = Physics.OverlapSphereNonAlloc(owner.transform.position, owner.DetectionRadius, m_ColliderBuffer, owner.DetectionLayer);
        if(detectedObjects > 0 && m_ColliderBuffer[0] != null)
        {
            owner.TargetObject = m_ColliderBuffer[0].gameObject;
            return true;
        }
        else
        {
            return false;
        }
    }
}
