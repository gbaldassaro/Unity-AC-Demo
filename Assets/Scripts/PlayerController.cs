using System;
using UnityEngine;

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
    public Transform debugTransform;
    #region Serialized Fields
    [Header("Player Input")]
    [SerializeField] private InputHandler input;

    [Header("Camera")]
    [SerializeField] private CameraController mainCamera;
    [SerializeField] private Transform cameraLockOn;

    [Header("Player Variables")]
    [Range(0,10)]
    [SerializeField] private float jumpVelocity;
    [Range(0,20)]
    [SerializeField] private float hoverMaxSpeed;
    [Range(0,10)]
    [SerializeField] private float walkMaxSpeed;
    [Range(0,20)]
    [SerializeField] private float boostMaxSpeed;
    [Range(20, 100)]
    [SerializeField] private float dashSpeed;
    [Range(-15,0)]
    [SerializeField] private float gravity;

    [Header("Player Movement Smoothing")]
    [Range(0,1)]
    [SerializeField] private float playerRotationSmoothTime;
    [Range(0,1)]
    [SerializeField] private float playerHorizontalVelocitySmoothTime;
    [Range(0,1)]
    [SerializeField] private float playerBoostVelocitySmoothTime;

    [Header("Arms")]
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftArm;
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

    #region Player Methods
    void MovePlayer()
    {
        // transition out of idle state
        if (input.moveInput != Vector2.zero)
        {
            switch (playerState)
            {
                case PlayerState.Idle:
                    playerState = PlayerState.Walking;
                    break;
            }
        } 

        SetHorizontalVelocity();
        SetVerticalVelocity();

        characterController.Move((horizontalVelocityVector + Vector3.up * verticalVelocity) * Time.deltaTime);

        PointPlayer();

        // transition into idle state
        if (desiredHorizontalVelocityVector.sqrMagnitude < 0.001f && 
            horizontalVelocityVector.sqrMagnitude < 0.001f && 
            verticalVelocity < 0.001f)
        {
            playerState = PlayerState.Idle;
        }

    }
    #endregion

    #region Helper Methods
    void SetHorizontalVelocity()
    {
        desiredHorizontalVelocityVector = Vector3.zero;

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

        desiredHorizontalVelocityVector = (forward.normalized * input.moveInput.y) + (right.normalized * input.moveInput.x);

        float maxSpeed = 0; 

        if (input.boostPressed)
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
            input.boostPressed = false;
        }
            
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

        if (horizontalVelocityVector.sqrMagnitude < 0.001f)
        {
            horizontalVelocityVector = Vector3.zero;
        }

        if (input.dashPressed)
        {
            horizontalVelocityVector = desiredHorizontalVelocityVector.normalized * dashSpeed;
            playerState = PlayerState.Boosting;
            input.dashPressed = false;
        }
    }

    void SetVerticalVelocity()
    {
        bool grounded = characterController.isGrounded;
        if (grounded)
        {
            verticalVelocity = 0f;
        }

        if (input.jumpHeld)
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
    }

    void PointPlayer()
    {
        Vector3 aimPoint = Vector3.zero;

        switch (mainCamera.cameraState)
        {
            case CameraState.FreeAim:
                // when firing, point player towards aim point
                if (input.shootRightHeld || input.shootLeftHeld)
                {
                    RaycastHit hit;
                    // if raycast hits something, aim at it
                    if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, 1000f)) 
                    {
                        Vector3 playerPosToAimAtPos = hit.point - transform.position;
                        playerPosToAimAtPos.y *= 0.2f;
                        transform.forward = Vector3.SmoothDamp(transform.forward, playerPosToAimAtPos, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                        
                        // makes arms aim a little behind target to not have bullets akwardly converge
                        aimPoint = hit.point + playerPosToAimAtPos.normalized * 2;
                        rightArm.transform.LookAt(aimPoint);
                        leftArm.transform.LookAt(aimPoint);
                    }

                    // if nothing ahead of player, aim far into the distance
                    else
                    {
                        aimPoint = mainCamera.transform.position + mainCamera.transform.forward * 100f;
                        rightArm.transform.LookAt(aimPoint);
                        leftArm.transform.LookAt(aimPoint);
                        aimPoint.y *= 0.2f;
                        transform.forward = Vector3.SmoothDamp(transform.forward, aimPoint, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                    }
                }
                
                // when not firing, point player towards movement
                else
                {
                    transform.forward = Vector3.SmoothDamp(transform.forward, desiredHorizontalVelocityVector, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                }
                break;

            case CameraState.LockedOn:
                // when locked on, point player at lock on target
                Vector3 playerPosToLookAtPos = cameraLockOn.position - transform.position;
                playerPosToLookAtPos.y *= 0.2f;
                transform.forward = Vector3.SmoothDamp(transform.forward, playerPosToLookAtPos, ref playerRotationSmoothVelocity, playerRotationSmoothTime);
                
                // makes arms aim a little behind target to not have bullets akwardly converge
                aimPoint = cameraLockOn.position + playerPosToLookAtPos.normalized * 2;
                rightArm.transform.LookAt(aimPoint);
                leftArm.transform.LookAt(aimPoint);
                break;
        }

        debugTransform.position = aimPoint;
    }
    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, horizontalVelocityVector);
    }
}
