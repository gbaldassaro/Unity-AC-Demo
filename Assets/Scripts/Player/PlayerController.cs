using System;
using System.Collections;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Boosting
}

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Player Input")]
    [SerializeField] private InputHandler input;

    [Header("Camera")]
    [SerializeField] private CameraController mainCamera;
    [SerializeField] private Transform lockOnPoint;
    [SerializeField] private Transform rightAimAtPoint;
    [SerializeField] private Transform leftAimAtPoint;
    private bool useLockOnMovement;

    [Header("Player Movement Variables")]
    [Range(0,10)] [SerializeField] private float jumpVelocity;
    [Range(0,20)] [SerializeField] private float hoverMaxSpeed;
    [Range(0,10)] [SerializeField] private float walkMaxSpeed;
    [Range(0,20)] public float boostMaxSpeed;
    [HideInInspector] public float maxSpeed;
    [Range(20, 100)] [SerializeField] private float dashSpeed;
    [Range(0, 1)] [SerializeField] private float dashDelay;
    [HideInInspector] public bool dashing;
    [Range(-15,0)] [SerializeField] private float gravity;

    private Vector3 desiredHorizontalVelocityVector = Vector3.forward;
    public Vector3 horizontalVelocityVector;
    public Vector3 localHorizontalVelocityVector;
    private float verticalVelocity;

    [Header("Player Movement Smoothing")]
    [Range(0,1)] [SerializeField] private float rotationSmoothTime;
    [Range(0,1)] [SerializeField] private float horizontalVelocitySmoothTime;
    [Range(0,1)] [SerializeField] private float hoverVelocitySmoothTime;
    private Vector3 playerRotationSmoothVelocity = new Vector3(0,0,0);
    private Vector3 playerHorizontalVelocitySmoothVelocity = new Vector3(0,0,0);
    private float playerVerticalVelocitySmoothVelocity = 0f;

    [Header("Arms")]
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftArm;

    [Header("Healing")]
    [SerializeField] private float healAmount;
    [SerializeField] private int maxHeals;
    [HideInInspector] public int healsLeft;
    private Health health;
    private PlayerState playerState;

    [Header("Energy")]
    [Range(0,200)] [SerializeField] public float maxEnergy;
    [Range(0,20)] [SerializeField] private float hoverEnergyPerSec;
    [Range(0,20)] [SerializeField] private float dashEnergy;
    public float currentEnergy;
    private float lastEnergyTime;
    [Range(0,5)] [SerializeField] private float energyRecoveryWaitTime;
    [Range(0,20)] [SerializeField] private float energyRecoveryPerSec;

    private bool startupFinished = false;

    #region Game Loop
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        healsLeft = maxHeals;
        currentEnergy = maxEnergy;
        playerState = PlayerState.Idle;

        StartCoroutine(Startup());

    }

    private IEnumerator Startup()
    {
        startupFinished = false;
        yield return new WaitForSecondsRealtime(3.0f);
        startupFinished = true;
    }

    void Update()
    {
        if (!startupFinished) return;

        MovePlayer();

        if (input.healPressed && healsLeft != 0)
        {
            HealPlayer();
        }

        TryFillEnergy();
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

        Vector3 right = mainCamera.transform.right;
        Vector3 forward = mainCamera.transform.forward;
        if (useLockOnMovement)
        {
            right = this.transform.right;
            forward = this.transform.forward;
        }

        right.y = 0;
        forward.y = 0;

        desiredHorizontalVelocityVector = (forward.normalized * input.moveInput.y) + (right.normalized * input.moveInput.x);
        desiredHorizontalVelocityVector = desiredHorizontalVelocityVector.normalized;

        Vector3 velocity = horizontalVelocityVector;
        Vector3 desiredVelocity = desiredHorizontalVelocityVector;
        if (useLockOnMovement)
        {
            velocity = localHorizontalVelocityVector;
            desiredVelocity = this.transform.InverseTransformDirection(desiredHorizontalVelocityVector);
        }

        maxSpeed = 0; 

        if (input.boostPressed && playerState == PlayerState.Walking)
        {
            playerState = PlayerState.Boosting;
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

        desiredVelocity *= maxSpeed;

        velocity = Vector3.SmoothDamp(velocity, desiredVelocity, ref playerHorizontalVelocitySmoothVelocity, horizontalVelocitySmoothTime);

        if (velocity.sqrMagnitude < 0.001f)
        {
            velocity = Vector3.zero;
        }

        if (input.dashPressed && currentEnergy - dashEnergy >= 0 &&
            velocity.sqrMagnitude > 0.001f && !dashing)
        {
            StartCoroutine(DashTimer());

            currentEnergy -= dashEnergy;
            lastEnergyTime = Time.time;

            velocity = desiredVelocity.normalized * dashSpeed;
            // horizontalVelocityVector = ((forward.normalized * input.moveInput.y) + (mainCamera.transform.right.normalized * input.moveInput.x)).normalized * dashSpeed;
            playerState = PlayerState.Boosting;
        }

        horizontalVelocityVector = velocity;
        localHorizontalVelocityVector = velocity;
        desiredHorizontalVelocityVector = desiredVelocity;
        if (useLockOnMovement)
        {
            horizontalVelocityVector = this.transform.TransformDirection(velocity);
            desiredHorizontalVelocityVector = this.transform.TransformDirection(desiredVelocity);
        }        
    }

    private IEnumerator DashTimer()
    {
        dashing = true;
        // slows player rotation while dashing
        float temp = rotationSmoothTime;
        rotationSmoothTime *= 2;

        float elapsedDashCooldownTime = 0;
        while (elapsedDashCooldownTime < dashDelay)
        {
            elapsedDashCooldownTime += Time.deltaTime;
            yield return null;
        }
        elapsedDashCooldownTime = 0;

        dashing = false;
        rotationSmoothTime = temp;
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
            else if (currentEnergy - hoverEnergyPerSec * Time.deltaTime >= 0)
            {
                currentEnergy -= hoverEnergyPerSec * Time.deltaTime;
                lastEnergyTime = Time.time;

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

                    this.transform.LookAt(new Vector3(aimPoint.x, this.transform.position.y, aimPoint.z));
                    // offset arm aim points to not make bullets converge to one spot
                    rightArm.transform.LookAt(aimPoint + mainCamera.transform.right * 0.1f);
                    leftArm.transform.LookAt(aimPoint - mainCamera.transform.right * 0.1f);
                }
                
                // when not firing, point player towards movement
                else
                {
                    Vector3 target = this.transform.forward;
                    target = Vector3.SmoothDamp(target, desiredHorizontalVelocityVector, ref playerRotationSmoothVelocity, rotationSmoothTime);
                    this.transform.forward = new Vector3(target.x, 0, target.z);
                    rightArm.transform.localRotation = Quaternion.identity;
                    leftArm.transform.localRotation = Quaternion.identity;
                }
                break;

            case CameraState.LockedOn:
                // when locked on, point player at aim at target
                Vector3 playerPosToLookAtPos = lockOnPoint.position - this.transform.position;
                playerPosToLookAtPos.y *= 0.2f;
                this.transform.forward = Vector3.SmoothDamp(this.transform.forward, playerPosToLookAtPos, ref playerRotationSmoothVelocity, rotationSmoothTime);
                
                // offset arm aim points to not make bullets converge to one spot
                rightArm.transform.LookAt(rightAimAtPoint.position + mainCamera.transform.right * 0.05f);
                leftArm.transform.LookAt(leftAimAtPoint.position - mainCamera.transform.right * 0.05f);
                break;
        }
    }

    public IEnumerator OnLockOn()
    {
        yield return new WaitForSeconds(rotationSmoothTime);
        useLockOnMovement = true;
        localHorizontalVelocityVector = this.transform.InverseTransformDirection(horizontalVelocityVector);
    }

    public void OnUnlock()
    {
        useLockOnMovement = false;
    }

    private void HealPlayer()
    {
        health.Heal(healAmount);
        healsLeft -= 1;
    }

    private void TryFillEnergy()
    {
        if (Time.time - lastEnergyTime > energyRecoveryWaitTime)
        {
            float recovery = energyRecoveryPerSec;
            if (characterController.isGrounded)
            {
                recovery *= 2.0f;
            }
            currentEnergy += recovery * Time.deltaTime;
        }

        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, desiredHorizontalVelocityVector);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(this.transform.position, horizontalVelocityVector);
    }
}
