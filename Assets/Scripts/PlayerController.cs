using System;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Boosting
}

public class PlayerController : MonoBehaviour
{
    public Transform debugTransform;

    [Header("Player Input")]
    [SerializeField] private InputHandler input;

    [Header("Camera")]
    [SerializeField] private CameraController mainCamera;
    [SerializeField] private Transform cameraLockOn;

    [Header("Player Movement Variables")]
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
    [SerializeField] private float rotationSmoothTime;
    [Range(0,1)]
    [SerializeField] private float horizontalVelocitySmoothTime;
    [Range(0,1)]
    [SerializeField] private float hoverVelocitySmoothTime;

    [Header("Arms")]
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftArm;

    [Header("Healing")]
    [SerializeField] private float healAmount;
    [SerializeField] private int maxHeals;
    [HideInInspector] public int healsLeft;
    
    private CharacterController characterController;
    private Health health;
    private PlayerState playerState;

    private Vector3 desiredHorizontalVelocityVector = Vector3.forward;
    private Vector3 horizontalVelocityVector;
    private float verticalVelocity;

    private Vector3 playerRotationSmoothVelocity = new Vector3(0,0,0);
    private Vector3 playerHorizontalVelocitySmoothVelocity = new Vector3(0,0,0);
    private float playerVerticalVelocitySmoothVelocity = 0f;

    #region Game Loop
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        healsLeft = maxHeals;
        playerState = PlayerState.Idle;
    }


    private void Update()
    {
        MovePlayer();

        if (input.healPressed && healsLeft != 0)
        {
            HealPlayer();
        }
    }
    #endregion

    #region Player Methods
    private void MovePlayer()
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
        if (desiredHorizontalVelocityVector.sqrMagnitude < 0.01f && 
            horizontalVelocityVector.sqrMagnitude < 0.01f && 
            verticalVelocity < 0.001f)
        {
            playerState = PlayerState.Idle;
        }

    }
    #endregion

    #region Helper Methods
    private void SetHorizontalVelocity()
    {
        desiredHorizontalVelocityVector = Vector3.zero;

        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0;

        Vector3 right = Vector3.zero; 
        switch (mainCamera.cameraState)
        {
            case CameraState.FreeAim:
            case CameraState.LockOnSearch:
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

        horizontalVelocityVector = Vector3.SmoothDamp(horizontalVelocityVector, desiredHorizontalVelocityVector, ref playerHorizontalVelocitySmoothVelocity, horizontalVelocitySmoothTime);

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

    private void SetVerticalVelocity()
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
                verticalVelocity = Mathf.SmoothDamp(verticalVelocity, hoverMaxSpeed, ref playerVerticalVelocitySmoothVelocity, hoverVelocitySmoothTime);
            }
            
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void PointPlayer()
    {
        Vector3 aimPoint;

        switch (mainCamera.cameraState)
        {
            case CameraState.FreeAim:
            case CameraState.LockOnSearch:
                // when firing, point player towards aim point
                if (input.shootRightHeld || input.shootLeftHeld)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, 100))
                    {
                        aimPoint = hit.point;
                    }
                    else
                    {
                        aimPoint = mainCamera.transform.position + mainCamera.transform.forward * 50f;
                    }

                    transform.LookAt(new Vector3(aimPoint.x, transform.position.y, aimPoint.z));
                    // offset arm aim points to not make bullets converge to one spot
                    rightArm.transform.LookAt(aimPoint + mainCamera.transform.right * 0.1f);
                    leftArm.transform.LookAt(aimPoint - mainCamera.transform.right * 0.1f);
                }
                
                // when not firing, point player towards movement
                else
                {
                    Vector3 target = transform.forward;
                    target = Vector3.SmoothDamp(target, desiredHorizontalVelocityVector, ref playerRotationSmoothVelocity, rotationSmoothTime);
                    transform.forward = new Vector3(target.x, 0, target.z);
                    rightArm.transform.localRotation = Quaternion.identity;
                    leftArm.transform.localRotation = Quaternion.identity;
                }
                break;

            case CameraState.LockedOn:
                // when locked on, point player at lock on target
                Vector3 playerPosToLookAtPos = cameraLockOn.position - transform.position;
                playerPosToLookAtPos.y *= 0.2f;
                transform.forward = Vector3.SmoothDamp(transform.forward, playerPosToLookAtPos, ref playerRotationSmoothVelocity, rotationSmoothTime);
                aimPoint = cameraLockOn.position;

                // offset arm aim points to not make bullets converge to one spot
                rightArm.transform.LookAt(aimPoint + mainCamera.transform.right * 0.1f);
                leftArm.transform.LookAt(aimPoint - mainCamera.transform.right * 0.1f);
                break;
        }
    }

    private void HealPlayer()
    {
        health.Heal(healAmount);
        healsLeft -= 1;
        input.healPressed = false;
    }
    #endregion
}
