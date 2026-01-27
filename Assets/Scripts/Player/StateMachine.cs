

public class StateMachine
{
    public PlayerState currentState { get; private set; }
    public bool canChangeState = true;
    public PlayerStateEnum currentStateType;
    public bool enable = true;
    public bool enableMovement = true;


    public void Initialize(PlayerState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        if (!canChangeState || !enable) return;

        currentState.Exit();
        currentState = newState;
        currentState.Enter();
        currentStateType = newState.type;
    }

    public void UpdateCurrent()
    {
        if (!enable) return;
        currentState.Update();
    }

    public void FixedUpdateCurrent()
    {
        if (!enable) return;
        currentState.FixedUpdate();
    }
}
