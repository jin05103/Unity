public class StateMachine
{
    private BaseState currentState;

    public void SetState(BaseState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update() => currentState?.Update();
}
