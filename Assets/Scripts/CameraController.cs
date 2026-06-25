using System;
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
    [Tooltip("How long, in seconds, the player must use the look input to break lock.")]
    [Range(0,1)] public float lockOnExitTime;
    [Range(0,5)] public float lockOnOffsetMagnitude;
    [Range(0,1)] public float offsetSmoothTime;
    private float targetOffset;
    private float offsetSmoothVelocity = 0f;
    private float dutch;
    public float dutchLimit;
    private float dutchSmoothVelocity;
    public float dutchSmoothTime;

    #region Game Loop
    private void Awake()
    {
        cameraState = CameraState.FreeAim;

        lockOnCinemachine = lockOnCamera.GetComponent<CinemachineCamera>();
        cinemachineFollow = lockOnCamera.GetComponent<CinemachineFollow>();
        orbitCinemachine = orbitCamera.GetComponent<CinemachineCamera>();

        targetOffset = lockOnOffsetMagnitude;
    }

    private void LateUpdate()
    {
        if (input.lockOnPressed)
        {
            switch (cameraState)
            {
                case CameraState.LockedOn:
                    cameraState = CameraState.FreeAim;
                    lockOnCamera.SetActive(false);
                    break;   

                case CameraState.LockOnSearch:
                    cameraState = CameraState.FreeAim;
                    break;   

                case CameraState.FreeAim:
                    cameraState = CameraState.LockOnSearch;
                    break;
            }
            input.lockOnPressed = false;
        }

        switch (cameraState)
        {
            case CameraState.LockOnSearch:
                FindLockOn();
                break;   
            
            case CameraState.LockedOn:
                UpdateLock();
                break;
        }

        MoveAim();
        TiltAndSlideCamera();
    }
    #endregion

    #region Camera Methods
    private void UpdateLock()
    {
        if (cameraState == CameraState.LockedOn)
        {
            // unlock if player uses any look input for long enough
            if (input.lookInput.sqrMagnitude > 0.001f)
            {
                lookTime += Time.deltaTime;
            }        
            else lookTime = 0;

            if (lookTime > lockOnExitTime)
            {
                cameraState = CameraState.FreeAim;
                lockOnCamera.SetActive(false);
                lookTime = 0;
            }
        }
        
        // switch target/search for new if current target dies
        if (currentLockOn == null)
        {
            cameraState = CameraState.LockOnSearch;
            lockOnCamera.SetActive(false);
            // immediately looks for new target to prevent camera snapping
            FindLockOn();
            lookTime = 0;
        }
    }

    private void MoveAim()
    {
        switch (cameraState)
        {
            case CameraState.LockedOn:
                lockOnPoint.position = Vector3.MoveTowards(lockOnPoint.position, currentLockOn.position, Time.deltaTime * 75);
                lockOnRotationControl.LookAt(lockOnPoint);
                break;

            case CameraState.FreeAim:
            case CameraState.LockOnSearch:
                lockOnPoint.position = transform.position + transform.forward * 10;
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
                if (Vector3.Dot(playerController.horizontalVelocityVector, transform.right) > 1)
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
        dutch = -0.02f * playerController.horizontalVelocityVector.magnitude * Vector3.Dot(playerController.horizontalVelocityVector, transform.right);
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
            // if enemy is closer to center than current best choice, continue
            Math.Abs(Vector3.Angle(transform.forward, hitCollider.transform.position - transform.position)) < minAngle &&
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
                        minAngle = Math.Abs(Vector3.Angle(transform.forward, hitCollider.transform.position - transform.position));
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
            currentEnemy = currentCandidate.GetComponent<Enemy>();
            // separate lock on tracking target rotation from player to prevent camera whipping when player turns around to lock on
            lockOnRotationControl.LookAt(currentLockOn);
            lockOnCamera.SetActive(true);
        }
    }
    #endregion
}
