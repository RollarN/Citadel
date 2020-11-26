using UnityEngine;

public class Attack : IState<AI>
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

        if (SqrDistanceFromPlayer(owner) < owner.SqrMaxAttackRange)
        {
            owner.AttackPlayer();
        }
    }

    private float SqrDistanceFromPlayer(AI owner)
    {
        return owner.SqrDistanceToPlayer(owner.transform.position, owner.PlayerTarget.transform.position);
    }
}