using System;
using UnityEngine;
using UnityEngine.InputSystem;

#region Enums
public enum PlayerState
{
    Idle,
    Walking,
    Boosting
}
#endregion

public class PlayerController : MonoBehaviour
{
    #region Public Fields
    [Header("Camera")]
    public Camera mainCamera;
    public Transform cameraLockOn;

    [Header("Player")]
    [Range(0,5)]
    public float jumpVelocity;
    [Range(0,20)]
    public float hoverMaxSpeed;
    [Range(0,1)]
    public float hoverAcceleration;
    [Range(0,10)]
    public float walkMaxSpeed;
    [Range(0,1)]
    public float walkAcceleration;
    [Range(0,20)]
    public float boostMaxSpeed;
    [Range(0,1)]
    public float boostAcceleration;
    [Range(0,1)]
    public float friction;
    [Range(-15,0)]
    public float gravity;

    [Header("Player Rotation Smoothing")]
    public Vector3 playerRotationSmoothVelocity = new Vector3(0,0,0);
    [Range(0,1)]
    public float playerRotationSmoothTime;
    #endregion
    
    #region Private Fields
    private CharacterController characterController;
    private PlayerState playerState;
    private CameraState cameraState;

    private Vector3 horizontalVelocityVector;
    private float verticalVelocity;
    private Vector3 accelerationVector;

    private bool jumpHeld;
    #endregion

    #region Input Fields
    private Vector2 moveInput;
    #endregion

    #region Game Loop
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerState = PlayerState.Idle;
        cameraState = CameraState.FreeAim;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        MovePlayer();
    }
    #endregion

    #region Input Methods
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpHeld = !jumpHeld;
        }
    }

    public void OnMove(InputAction.CallbackContext context) 
    {
        moveInput = context.ReadValue<Vector2>();
        switch (playerState)
        {
            case PlayerState.Idle:
                playerState = PlayerState.Walking;
                break;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        Debug.Log("Dash");
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        switch (playerState)
        {
            case PlayerState.Walking:
                Debug.Log("Boosting");
                playerState = PlayerState.Boosting;
                break;
            case PlayerState.Boosting:
                Debug.Log("Walking");
                playerState = PlayerState.Walking;
                break;
        }
    }

    public void OnLockOn(InputAction.CallbackContext context)
    {
        if (context.performed){
            switch (cameraState)
            {
                case CameraState.FreeAim:
                    cameraState = CameraState.LockedOn;
                    break;
                case CameraState.LockedOn:
                    cameraState = CameraState.FreeAim;
                    break;
            }
        }
    }
    #endregion

    #region Player Methods
    void MovePlayer()
    {
        if (playerState != PlayerState.Idle)
        {
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0;
            Vector3 right = mainCamera.transform.right;
            right.y = 0;
            accelerationVector = (forward.normalized * moveInput.y) + (right.normalized * moveInput.x);

            float maxSpeed = 0; 

            switch (playerState)
            {
                case PlayerState.Walking:
                    maxSpeed = walkMaxSpeed;
                    accelerationVector *= walkAcceleration;
                    break;
                case PlayerState.Boosting:
                    maxSpeed = boostMaxSpeed;
                    accelerationVector *= boostAcceleration;
                    break;
            }

            if (moveInput.sqrMagnitude < 0.001f)
            {
                accelerationVector = -horizontalVelocityVector * friction;
            }

            horizontalVelocityVector += accelerationVector;

            if (horizontalVelocityVector.magnitude > maxSpeed)
            {
                horizontalVelocityVector = horizontalVelocityVector.normalized * maxSpeed;
            }

            if (horizontalVelocityVector.magnitude < 0.01f)
            {
                horizontalVelocityVector = Vector3.zero;
                playerState = PlayerState.Idle;
            }
        }

        switch (cameraState){
            case CameraState.FreeAim:
                transform.forward = Vector3.SmoothDamp(transform.forward, horizontalVelocityVector, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                break;
            case CameraState.LockedOn:
                Vector3 playerPosToLookAtPos = cameraLockOn.position - transform.position;
                transform.forward = Vector3.SmoothDamp(transform.forward, playerPosToLookAtPos, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                break;
        }

        bool grounded = characterController.isGrounded;
        if (grounded)
        {
            verticalVelocity = 0f;
        }

        if (jumpHeld)
        {
            if (grounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpVelocity * -2f * gravity);
            }
            else if (verticalVelocity + hoverAcceleration < hoverMaxSpeed)
            {
                verticalVelocity += hoverAcceleration;
            }
            
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move((horizontalVelocityVector + Vector3.up * verticalVelocity) * Time.deltaTime);

    }

    #endregion

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        // Draw a line representing a vector from the object's position
        Gizmos.DrawRay(transform.position, horizontalVelocityVector);
    }
}
