using UnityEngine;

public class Follow : IState<AI>
{

    #region Enter & Exit

    public void EnterState(AI owner)
    {
    }

    public void ExitState(AI owner)
    {
    }

    #endregion Enter & Exit

    public void UpdateState(AI owner)
    {
        if (!owner.PlayerTarget)
        {
            owner.SetState(owner.IdleState);
            return;
        }

        if (!owner.CanSeePlayer)
        {
            owner.SetState(owner.SearchState);
            return;
        }

        owner.FollowMove();

        if (owner.SqrDistanceToPlayer(owner.transform.position, owner.PlayerTarget.transform.position) <= owner.SqrAttackRange)
        {
            owner.SetState(owner.AttackState);
            return;
        }
    }
}