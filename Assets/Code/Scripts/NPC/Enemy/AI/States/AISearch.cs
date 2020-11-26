using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Makes the owner walk to the last known location of the target, 
/// until it sees the target or has reached the destination
/// </summary>
public class AISearch : IState<AI>
{
    //Put in AI script instead so it can share with and be updated by attack state
    private float m_DistanceToTarget = 0f;
    public void EnterState(AI owner)
    {
        if (owner.Agent.isOnNavMesh)
        {
            UpdateSearchPath(owner);
        }
    }

    public void ExitState(AI owner)
    {
        
    }

    public void UpdateState(AI owner)
    {
        if (owner.CanSeePlayer)
        {
            //owner.Agent.isStopped = true;
            owner.SetState(owner.FollowState);
            return;
        }

        if
        (
            m_DistanceToTarget != Mathf.Infinity 
            && owner.Agent.pathStatus == NavMeshPathStatus.PathComplete 
            && owner.Agent.remainingDistance == 0
        )
        {
            owner.SetState(owner.IdleState);
        }

        if(owner.Agent.pathStatus == NavMeshPathStatus.PathInvalid && owner.Agent.isOnNavMesh)
        {
            UpdateSearchPath(owner);
        }
    }

    private void UpdateSearchPath(AI owner)
    {
        owner.SetDestinationNearPlayer(owner.LastKnownTargetPosition);
        owner.GoToNewDestination();
        m_DistanceToTarget = owner.Agent.remainingDistance;
    }
}
