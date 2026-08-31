using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public enum CameraState
{
    LockOnSearch,
    LockedOn,
    FreeAim
}

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public CameraState cameraState;

    [Header("Player Input")]
    [SerializeField] private InputHandler input;
    private float lookTime;

    [Header("Orbit")]
    [SerializeField] private GameObject orbitCamera;
    private CinemachineCamera orbitCinemachine;

    [Header("Lock On")]
    [SerializeField] private GameObject lockOnCamera;
    private CinemachineCamera lockOnCinemachine;
    private CinemachineFollow cinemachineFollow;
    [SerializeField] private Transform lockOnRotationControl;
    [SerializeField] private Transform lockOnPoint;
    [SerializeField] private float lockOnRange;
    private Transform currentLockOn;
    [HideInInspector] public Enemy currentEnemy;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerController playerController;

    [Header("Camera Variables")]
    [SerializeField] private float lockOnExitThreshold;
    [Range(0,5)] [SerializeField] private float lockOnOffsetMagnitude;
    [Range(0,1)] [SerializeField] private float offsetSmoothTime;
    private float targetOffset;
    private float offsetSmoothVelocity = 0f;
    private float dutch;
    [SerializeField] private float dutchLimit;
    private float dutchSmoothVelocity;
    [SerializeField] private float dutchSmoothTime;

    private Transform lastLockOn;

    private bool startupFinished = false;

    private bool justLocked;

    #region Game Loop
    private void Awake()
    {
        cameraState = CameraState.FreeAim;

        lockOnCinemachine = lockOnCamera.GetComponent<CinemachineCamera>();
        cinemachineFollow = lockOnCamera.GetComponent<CinemachineFollow>();
        orbitCinemachine = orbitCamera.GetComponent<CinemachineCamera>();

        targetOffset = lockOnOffsetMagnitude;
    }

    private void Start()
    {
        StartCoroutine(Startup());
    }

    private void Update()
    {
        if (!startupFinished)
        {
            return;
        }

        if (input.lockOnPressed)
        {
            switch (cameraState)
            {
                case CameraState.LockedOn:
                    cameraState = CameraState.FreeAim;
                    lockOnCamera.SetActive(false);
                    lastLockOn = null;
                    playerController.OnUnlock();
                    break;   

                case CameraState.LockOnSearch:
                    cameraState = CameraState.FreeAim;
                    break;   

                case CameraState.FreeAim:
                    cameraState = CameraState.LockOnSearch;
                    break;
            }
        }
    }

    private void LateUpdate()
    {
        if (!startupFinished) return;

        UpdateLock();
        MoveAim();
        TiltAndSlideCamera();
    }
    #endregion

    #region Camera Methods
    private IEnumerator Startup()
    {
        orbitCamera.SetActive(false);
        lockOnCamera.SetActive(false);
        orbitCinemachine.BlendHint &= ~CinemachineCore.BlendHints.InheritPosition;
        yield return new WaitForSecondsRealtime(1.0f);
        orbitCamera.SetActive(true);
        yield return new WaitForSecondsRealtime(2.0f);
        orbitCinemachine.BlendHint |= CinemachineCore.BlendHints.InheritPosition;
        startupFinished = true;
    }

    private void UpdateLock()
    {
        switch (cameraState)
        {
            case (CameraState.LockedOn):
                // relock if player uses look input beyond threshold
                if (input.lookInput.sqrMagnitude > 1000.0f && !justLocked)
                {
                    SwitchLockOn();
                }
                // allows player to switch locks only after having no camera input on current lock
                if (input.lookInput.sqrMagnitude < 0.001f)
                {
                    justLocked = false;
                }
            
                // switch target/search for new if current target dies
                if (currentLockOn == null)
                {
                    cameraState = CameraState.LockOnSearch;
                    lockOnCamera.SetActive(false);
                    // immediately looks for new target to prevent camera snapping
                    FindLockOn();
                }

                break;

            case (CameraState.LockOnSearch):
                if (input.lookInput.sqrMagnitude < 0.001f)
                {
                    FindLockOn();
                }
                break;
        }
        
    }

    private void MoveAim()
    {
        switch (cameraState)
        {
            case CameraState.LockedOn:
                lockOnPoint.position = Vector3.MoveTowards(lockOnPoint.position, currentLockOn.position, Time.deltaTime * 200);
                lockOnRotationControl.LookAt(lockOnPoint);
                break;

            case CameraState.FreeAim:
            case CameraState.LockOnSearch:
                lockOnPoint.position = this.transform.position + this.transform.forward * 10;
                break;
        }
    }

    private void TiltAndSlideCamera()
    {
        if (cameraState == CameraState.LockedOn)
        {
            // if moving fast enough, move camera side
            if (playerController.horizontalVelocityVector.magnitude > playerController.boostMaxSpeed * 0.75)
            {
                // if moving to the right relative to the camera, move camera to left shoulder
                if (Vector3.Dot(playerController.horizontalVelocityVector, this.transform.right) > 1)
                {
                    targetOffset = -1 * lockOnOffsetMagnitude;
                }
                // if moving to the left relative to the camera, move camera to left shoulder
                else
                {
                    targetOffset = lockOnOffsetMagnitude;    
                }
            }
            cinemachineFollow.FollowOffset.x = Mathf.SmoothDamp(cinemachineFollow.FollowOffset.x, targetOffset, ref offsetSmoothVelocity, offsetSmoothTime);
        }

        // tilts camera on left and right movement
        dutch = -0.02f * playerController.horizontalVelocityVector.magnitude * Vector3.Dot(playerController.horizontalVelocityVector, this.transform.right);
        dutch = Mathf.Clamp(dutch, -dutchLimit, dutchLimit);
        Mathf.SmoothDamp(lockOnCinemachine.Lens.Dutch, dutch, ref dutchSmoothVelocity, dutchSmoothTime);
        lockOnCinemachine.Lens.Dutch = Mathf.SmoothDamp(lockOnCinemachine.Lens.Dutch, dutch, ref dutchSmoothVelocity, dutchSmoothTime);
        orbitCinemachine.Lens.Dutch = Mathf.SmoothDamp(orbitCinemachine.Lens.Dutch, dutch, ref dutchSmoothVelocity, dutchSmoothTime);
    }

    private void FindLockOn()
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, lockOnRange);
        float minAngle = Mathf.Infinity;
        Vector3 viewportPos;
        Collider currentCandidate = null;
        foreach (var hitCollider in hitColliders)
        {
            // gets collider's position within screen space
            viewportPos = mainCamera.WorldToViewportPoint(hitCollider.transform.position);

            // if collider is enemy, continue
            if (hitCollider.CompareTag("Enemy") &&
            // if collider is not currently locked on enemy, continue
            hitCollider.transform.Find("Lock On Point") != lastLockOn && 
            // if enemy is closer to center than current best choice, continue
            Math.Abs(Vector3.Angle(this.transform.forward, hitCollider.transform.position - this.transform.position)) < minAngle &&
            // if enemy is within screen (with small padding), continue
            viewportPos.x > 0.05f && viewportPos.x < 0.95f && viewportPos.y > 0.05f && viewportPos.y < 0.95f && viewportPos.z > 0)
            {
                Vector3 directionToEnemy = hitCollider.transform.position - mainCamera.transform.position;
                float distanceToEnemy = directionToEnemy.magnitude;
                // if enemy is not blocked from view by obstacle, enemy is current best lock on choice
                if (Physics.Raycast(mainCamera.transform.position, directionToEnemy, out RaycastHit hit, distanceToEnemy))
                {
                    if (hit.transform == hitCollider.transform)
                    {
                        minAngle = Math.Abs(Vector3.Angle(this.transform.forward, hitCollider.transform.position - this.transform.position));
                        currentCandidate = hitCollider;
                    }
                }
            }
        }

        if (currentCandidate != null)
        {
            cameraState = CameraState.LockedOn;
            // lock on is separate from enemy to allow custom lock on placement 
            currentLockOn = currentCandidate.transform.Find("Lock On Point");
            lastLockOn = currentLockOn;
            currentEnemy = currentCandidate.GetComponent<Enemy>();
            // separate lock on tracking target rotation from player to prevent camera whipping when player turns around to lock on
            lockOnRotationControl.LookAt(currentLockOn);
            lockOnCamera.SetActive(true);

            justLocked = true;
            StartCoroutine(playerController.OnLockOn());
        }
        else
        {
            currentLockOn = null;
            lastLockOn = null;
            playerController.OnUnlock();
        }
    }

    private void SwitchLockOn()
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, lockOnRange);
        float minAngle = Mathf.Infinity;
        // minimum angle between input direction and enemy direction from center of screen
        float minInputAngle = Mathf.Infinity;
        Vector3 viewportPos;
        Collider currentCandidate = null;
        foreach (var hitCollider in hitColliders)
        {
            // gets collider's position within screen space
            viewportPos = mainCamera.WorldToViewportPoint(hitCollider.transform.position);

            // if collider is enemy, continue
            if (hitCollider.CompareTag("Enemy") &&
            // if collider is not currently locked on enemy, continue
            hitCollider.transform.Find("Lock On Point") != lastLockOn && 
            // if collider is in direction of camera input, continue
            Vector2.Dot(input.lookInput, new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f)) > 0 &&
            // if enemy direction is closer to look input, continue
            Math.Abs(Vector2.Angle(input.lookInput, new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f))) < minInputAngle &&
            // if enemy is closer to center than current best choice, continue
            Math.Abs(Vector3.Angle(this.transform.forward, hitCollider.transform.position - this.transform.position)) < minAngle &&
            // if enemy is within screen (with small padding), continue
            viewportPos.x > 0.05f && viewportPos.x < 0.95f && viewportPos.y > 0.05f && viewportPos.y < 0.95f && viewportPos.z > 0)
            {
                Vector3 directionToEnemy = hitCollider.transform.position - mainCamera.transform.position;
                float distanceToEnemy = directionToEnemy.magnitude;
                // if enemy is not blocked from view by obstacle, enemy is current best lock on choice
                if (Physics.Raycast(mainCamera.transform.position, directionToEnemy, out RaycastHit hit, distanceToEnemy))
                {
                    if (hit.transform == hitCollider.transform)
                    {
                        minInputAngle = Math.Abs(Vector2.Angle(input.lookInput, new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f)));
                        minAngle = Math.Abs(Vector3.Angle(this.transform.forward, hitCollider.transform.position - this.transform.position));
                        currentCandidate = hitCollider;
                    }
                }
            }
        }

        if (currentCandidate != null)
        {
            cameraState = CameraState.LockedOn;
            // lock on is separate from enemy to allow custom lock on placement 
            currentLockOn = currentCandidate.transform.Find("Lock On Point");
            lastLockOn = currentLockOn;
            currentEnemy = currentCandidate.GetComponent<Enemy>();
            // separate lock on tracking target rotation from player to prevent camera whipping when player turns around to lock on
            lockOnRotationControl.LookAt(currentLockOn);
            lockOnCamera.SetActive(true);

            justLocked = true;
            StartCoroutine(playerController.OnLockOn());
        }
        else
        {
            currentLockOn = null;
            lastLockOn = null;
            playerController.OnUnlock();
        }
    }
    #endregion
}
