[System.Serializable]
public class StateMachine<T>
{
    public IState<T> currentState { get; private set; }
    public T owner;

    public StateMachine(T owner)
    {
        this.owner = owner;
        currentState = null;
    }

    public void ChangeState(IState<T> newState)
    {
        currentState?.ExitState(owner);

        currentState = newState;
        currentState.EnterState(owner);
    }

    public void UpdateState()
    {
        currentState?.UpdateState(owner);
    }
}

public interface IState<T>
{
    void EnterState(T owner);
    void ExitState(T owner);
    void UpdateState(T owner);
}