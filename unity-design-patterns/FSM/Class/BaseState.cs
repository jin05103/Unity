public abstract class BaseState
{
    protected PlayerController player;
    public BaseState(PlayerController player) { this.player = player; }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
