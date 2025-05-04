using UnityEngine;

public class State_Idle : IState
{
    private PlayerController player;
    public State_Idle(PlayerController player) { this.player = player; }

    public void Enter() => Debug.Log("Idle Enter");
    public void Update()
    {
        if (Input.GetKey(KeyCode.W))
            player.stateMachine.SetState(new State_Run(player));
    }
    public void Exit() => Debug.Log("Idle Exit");
}
