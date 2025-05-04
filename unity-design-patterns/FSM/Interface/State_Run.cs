using UnityEngine;

public class State_Run : IState
{
    private PlayerController player;
    public State_Run(PlayerController player) { this.player = player; }

    public void Enter() => Debug.Log("Run Enter");
    public void Update()
    {
        if (!Input.GetKey(KeyCode.W))
            player.stateMachine.SetState(new State_Idle(player));
    }
    public void Exit() => Debug.Log("Run Exit");
}
