using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 lookInput;

    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool shootRightHeld;
    [HideInInspector] public double shootRightHeldStartTime;
    [HideInInspector] public bool shootLeftHeld;
    [HideInInspector] public double shootLeftHeldStartTime;
    [HideInInspector] public bool rightPressed;
    [HideInInspector] public bool leftPressed;
    [HideInInspector] public bool shiftControlHeld;

    [HideInInspector] public bool dashPressed;
    [HideInInspector] public bool boostPressed;
    [HideInInspector] public bool lockOnPressed;
    [HideInInspector] public bool healPressed;
    [HideInInspector] public bool interactPressed;

    void Start()
    {
        jumpHeld = false;
        shootRightHeld = false;
        shootLeftHeld = false;
    }

    #region Vector Inputs
    public void OnMove(InputAction.CallbackContext context) 
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    #endregion

    #region Held Inputs
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpHeld = true;
        }
        if (context.canceled)
        {
            jumpHeld = false;
        }
    }

    public void OnShootRight(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            shootRightHeld = true;
            shootRightHeldStartTime = context.startTime;
        }
        if (context.canceled)
        {
            shootRightHeld = false;
        }
    }

    public void OnShootLeft(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            shootLeftHeld = true;
            shootLeftHeldStartTime = context.startTime;
        }
        if (context.canceled)
        {
            shootLeftHeld = false;
        }
    }

    public void OnShiftControl(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            shiftControlHeld = true;
        }
        if (context.canceled)
        {
            shiftControlHeld = false;
        }
    }
    #endregion

    #region Pressed Inputs
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dashPressed = true;
        }
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            boostPressed = true;
        }
    }

    public void OnLockOn(InputAction.CallbackContext context)
    {
        if (context.performed){
            lockOnPressed = true;
        }    
    }

    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            healPressed = true;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactPressed = true;
        }
    }

    public void OnRightPress(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            rightPressed = true;
        }
    }

    public void OnLeftPress(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leftPressed = true;
        }
    }
    #endregion
}