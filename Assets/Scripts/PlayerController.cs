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
    #region Serialized Fields
    [Header("Camera")]
    [SerializeField] private CameraController mainCamera;
    [SerializeField] private Transform cameraLockOn;

    [Header("Player")]
    [Range(0,10)]
    public float jumpVelocity;
    [Range(0,20)]
    public float hoverMaxSpeed;
    [Range(0,10)]
    public float walkMaxSpeed;
    [Range(0,20)]
    public float boostMaxSpeed;
    [Range(20, 100)]
    public float dashSpeed;
    [Range(-15,0)]
    public float gravity;

    [Header("Player Movement Smoothing")]
    [Range(0,1)]
    public float playerRotationSmoothTime;
    [Range(0,1)]
    public float playerHorizontalVelocitySmoothTime;
    [Range(0,1)]
    public float playerBoostVelocitySmoothTime;

    [Header("Weapons")]
    [SerializeField] private RangedWeaponController rightHandWeapon;
    [SerializeField] private RangedWeaponController leftHandWeapon;
    #endregion
    
    #region Private Fields
    private CharacterController characterController;
    private PlayerState playerState;

    private Vector3 desiredHorizontalVelocityVector = Vector3.forward;
    private Vector3 horizontalVelocityVector;
    private float verticalVelocity;

    private Vector3 playerRotationSmoothVelocity = new Vector3(0,0,0);
    private Vector3 playerHorizontalVelocitySmoothVelocity = new Vector3(0,0,0);
    private float playerVerticalVelocitySmoothVelocity = 0f;

    private bool jumpHeld;
    #endregion

    #region Input Fields
    public Vector2 moveInput;
    #endregion

    #region Game Loop
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerState = PlayerState.Idle;
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
        if (context.performed)
        {
            horizontalVelocityVector = desiredHorizontalVelocityVector.normalized * dashSpeed;
            playerState = PlayerState.Boosting;
        }
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        switch (playerState)
        {
            case PlayerState.Walking:
                playerState = PlayerState.Boosting;
                break;
            case PlayerState.Boosting:
                playerState = PlayerState.Walking;
                break;
        }
    }
    #endregion

    #region Player Methods
    void MovePlayer()
    {
        desiredHorizontalVelocityVector = Vector3.zero;
        
        if (playerState != PlayerState.Idle)
        {
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0;

            Vector3 right = Vector3.zero; 
            switch (mainCamera.cameraState)
            {
                case CameraState.FreeAim:
                    right = mainCamera.transform.right;
                    break;
                case CameraState.LockedOn:
                    right = transform.right;
                    break;
            }
            right.y = 0;

            desiredHorizontalVelocityVector = (forward.normalized * moveInput.y) + (right.normalized * moveInput.x);

            float maxSpeed = 0; 

            switch (playerState)
            {
                case PlayerState.Walking:
                    maxSpeed = walkMaxSpeed;
                    break;
                case PlayerState.Boosting:
                    maxSpeed = boostMaxSpeed;
                    break;
            }

            desiredHorizontalVelocityVector *= maxSpeed;

            horizontalVelocityVector = Vector3.SmoothDamp(horizontalVelocityVector, desiredHorizontalVelocityVector, ref playerHorizontalVelocitySmoothVelocity, playerHorizontalVelocitySmoothTime);

            if (horizontalVelocityVector.magnitude < 0.01f)
            {
                horizontalVelocityVector = Vector3.zero;
                playerState = PlayerState.Idle;
            }
        }

        switch (mainCamera.cameraState)
        {
            case CameraState.FreeAim:
                transform.forward = Vector3.SmoothDamp(transform.forward, horizontalVelocityVector, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                break;
            case CameraState.LockedOn:
                Vector3 playerPosToLookAtPos = cameraLockOn.position - transform.position;
                playerPosToLookAtPos.y *= 0.2f;
                transform.forward = Vector3.SmoothDamp(transform.forward, playerPosToLookAtPos, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                rightHandWeapon.gunModel.transform.LookAt(cameraLockOn.position);
                leftHandWeapon.gunModel.transform.LookAt(cameraLockOn.position);
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
            else
            {
                verticalVelocity = Mathf.SmoothDamp(verticalVelocity, hoverMaxSpeed, ref playerVerticalVelocitySmoothVelocity, playerBoostVelocitySmoothTime);
            }
            
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move((horizontalVelocityVector + Vector3.up * verticalVelocity) * Time.deltaTime);

    }

    #endregion

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, horizontalVelocityVector);
    }
}
