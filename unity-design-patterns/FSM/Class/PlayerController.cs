using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public StateMachine stateMachine { get; private set; }

    void Start()
    {
        stateMachine = new StateMachine();
        stateMachine.SetState(new State_Idle(this));
    }

    void Update()
    {
        stateMachine.Update();
    }
}
