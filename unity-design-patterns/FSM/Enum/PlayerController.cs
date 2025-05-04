using UnityEngine;

public enum PlayerState
{
    Idle,
    Run,
    Attack
}

public class PlayerController : MonoBehaviour
{
    private PlayerState currentState;

    void Start()
    {
        currentState = PlayerState.Idle;
    }

    void Update()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                if (Input.GetKey(KeyCode.W)) currentState = PlayerState.Run;
                break;

            case PlayerState.Run:
                if (!Input.GetKey(KeyCode.W)) currentState = PlayerState.Idle;
                break;

            case PlayerState.Attack:
                currentState = PlayerState.Idle;
                break;
        }
    }
}