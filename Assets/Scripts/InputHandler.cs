using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 lookInput;

    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool shootRightHeld;
    [HideInInspector] public bool shootLeftHeld;

    [HideInInspector] public bool dashPressed;
    [HideInInspector] public bool boostPressed;
    [HideInInspector] public bool lockOnPressed;

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
        if (context.performed)
        {
            jumpHeld = !jumpHeld;
        }
    }

    public void OnShootRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            shootRightHeld = !shootRightHeld;
        }
    }

    public void OnShootLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            shootLeftHeld = !shootLeftHeld;
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
    #endregion
}