using UnityEngine;

public class State_Run : BaseState
{
    public State_Run(PlayerController player) : base(player) { }

    public override void Update()
    {
        if (!Input.GetKey(KeyCode.W))
            player.stateMachine.SetState(new State_Idle(player));
    }
}
