using UnityEngine;

public class Idle : IState<AI>
{
    private readonly Collider[] m_ColliderBuffer = new Collider[1];
    private bool m_ChangeToFollowState = false;

    public void EnterState(AI owner)
    {
        owner.OnTag += ChangeToFollowState;
        m_ChangeToFollowState = false;
    }

    public void ExitState(AI owner)
    {
        owner.OnTag -= ChangeToFollowState;
        m_ChangeToFollowState = false;
    }

    public void UpdateState(AI owner)
    {
        if (m_ChangeToFollowState || DetectPlayer(owner))
        {
            owner.SetState(owner.FollowState);
        }
    }

    private void ChangeToFollowState()
    {
        m_ChangeToFollowState = true;
    }

    private bool DetectPlayer(AI owner)
    {
        return (owner.PlayerTarget != null || DetectPlayerBySphere(owner)) && owner.CanSeePlayer;
    }

    private bool DetectPlayerBySphere(AI owner)
    {
        int detectedObjects = Physics.OverlapSphereNonAlloc(owner.transform.position, owner.PlayerDetectionRadius, m_ColliderBuffer, owner.PlayerLayer);

        if (detectedObjects > 0 && m_ColliderBuffer[0] != null)
        {
            owner.PlayerTarget = m_ColliderBuffer[0].gameObject;
            return true;
        }
        else
        {
            return false;
        }
    }

}