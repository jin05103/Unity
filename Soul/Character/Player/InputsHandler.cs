using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputsHandler : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool roll;
    public bool jump;
    public bool sprint;
    public bool leftClick;
    public bool rightClick;
    public bool escape;
    public bool parry;
    public bool itemUse;

    bool pendingLeftClick;
    bool pendingRightClick;

    public bool dUp;
    public bool dDown;
    public bool dLeft;
    public bool dRight;
    public bool handChange;
    public bool interact;
    public bool interactLeft;
    public bool interactRight;
    

    public bool lockOn;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    private void Update() {
        if (pendingLeftClick)
        {
            // leftClick = (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject());
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                leftClick = true;
            }
            pendingLeftClick = false;
        }
        if (pendingRightClick)
        {
            // rightClick = (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject());
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                rightClick = true;
            }
            pendingRightClick = false;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (cursorInputForLook)
        {
            look = context.ReadValue<Vector2>();
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll = context.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jump = context.ReadValueAsButton();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        sprint = context.ReadValueAsButton();
    }

    public void OnLockOn(InputAction.CallbackContext context)
    {
        lockOn = context.ReadValueAsButton();
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pendingLeftClick = true;
        }
        else if (context.canceled)
        {
            pendingLeftClick = false;
            leftClick = false;
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pendingRightClick = true;
        }
        else if (context.canceled)
        {
            pendingRightClick = false;
            rightClick = false;
        }
    }

    public void OnDpadUp(InputAction.CallbackContext context)
    {
        dUp = context.ReadValueAsButton();
    }

    public void OnDpadDown(InputAction.CallbackContext context)
    {
        dDown = context.ReadValueAsButton();
    }

    public void OnDpadLeft(InputAction.CallbackContext context)
    {
        dLeft = context.ReadValueAsButton();
    }

    public void OnDpadRight(InputAction.CallbackContext context)
    {
        dRight = context.ReadValueAsButton();
    }

    public void OnHandChange(InputAction.CallbackContext context)
    {
        handChange = context.ReadValueAsButton();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        interact = context.ReadValueAsButton();
    }

    public void OnInteractLeft(InputAction.CallbackContext context)
    {
        interactLeft = context.ReadValueAsButton();
    }

    public void OnInteractRight(InputAction.CallbackContext context)
    {
        interactRight = context.ReadValueAsButton();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        escape = context.ReadValueAsButton();
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        parry = context.ReadValueAsButton();
    }

    public void OnItemUse(InputAction.CallbackContext context)
    {
        itemUse = context.ReadValueAsButton();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
