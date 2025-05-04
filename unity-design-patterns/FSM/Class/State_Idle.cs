using UnityEngine;

public class State_Idle : BaseState
{
    public State_Idle(PlayerController player) : base(player) { }

    public override void Update()
    {
        if (Input.GetKey(KeyCode.W))
            player.stateMachine.SetState(new State_Run(player));
    }
}
