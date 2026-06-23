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

    [Header("Player Input")]
    [SerializeField] private InputHandler input;

    [Header("Lock On")]
    [SerializeField] private GameObject lockOnCamera;
    public Transform lockOnLookAt;
    [SerializeField] private float lockOnRange;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerController playerController;

    [Header("Camera Variables")]
    [Range(0,1)]
    [Tooltip("How long, in seconds, the player must use the look input to break lock.")]
    public float lockOnExitTime;
    [Range(0,5)]
    public float lockOnOffsetMagnitude;
    [Range(0,1)]
    public float offsetSmoothTime;

    public CameraState cameraState;

    private Transform currentLockOn;
    private float lookTime;

    private CinemachineFollow cinemachineFollow;
    private float targetOffset;

    private float offsetSmoothVelocity = 0f;

    #region Game Loop
    private void Awake()
    {
        cameraState = CameraState.FreeAim;

        cinemachineFollow = lockOnCamera.GetComponent<CinemachineFollow>();
        targetOffset = lockOnOffsetMagnitude;
    }

    private void Update()
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
                TryBreakLock();
                break;
        }

        MoveLookAt();
    }
    #endregion

    #region Camera Methods
    private void TryBreakLock()
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

    private void MoveLookAt()
    {
        switch (cameraState)
        {
            case CameraState.LockedOn:
                if (currentLockOn != null){
                    lockOnLookAt.position = Vector3.MoveTowards(lockOnLookAt.position, currentLockOn.position, Time.deltaTime * 100);
                }
                break;

            case CameraState.FreeAim:
            case CameraState.LockOnSearch:
                lockOnLookAt.position = transform.position + transform.forward * 10;
                break;
        }


        if (input.moveInput.x > 0)
        {
            targetOffset = -1 * lockOnOffsetMagnitude;
        }
        else if (input.moveInput.x < 0)
        {
            targetOffset = lockOnOffsetMagnitude;
        }
        cinemachineFollow.FollowOffset.x = Mathf.SmoothDamp(cinemachineFollow.FollowOffset.x, targetOffset, ref offsetSmoothVelocity, offsetSmoothTime);
    }

    private void FindLockOn()
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, lockOnRange);
        float minAngle = Mathf.Infinity;
        Collider currentCandidate = null;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Lock On Point") && 
            Math.Abs(Vector3.Angle(transform.forward, hitCollider.transform.position - transform.position)) < minAngle)
            {
                minAngle = Math.Abs(Vector3.Angle(transform.forward, hitCollider.transform.position - transform.position));
                currentCandidate = hitCollider;
            }
        }

        if (currentCandidate != null)
        {
            cameraState = CameraState.LockedOn;
            currentLockOn = currentCandidate.transform;
            lockOnCamera.SetActive(true);
        }
    }
    #endregion
}
