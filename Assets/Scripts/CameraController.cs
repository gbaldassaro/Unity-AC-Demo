using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

#region Enums
public enum CameraState
{
    LockedOn,
    FreeAim
}
#endregion

public class CameraController : MonoBehaviour
{

    #region Serialized Fields
    [SerializeField] private GameObject lockOnCamera;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform lockOnLookAt;
    #endregion

    #region Public Fields
    public CameraState cameraState;
    
    [Range(0,1)]
    [Tooltip("How long, in seconds, the player must use the look input to break lock.")]
    public float lockOnExitTime;

    [Range(0,5)]
    public float lockOnOffsetMagnitude;
    [Range(0,1)]
    public float offsetSmoothTime;
    #endregion

    #region Private Fields
    private Vector2 lookInput;
    private Transform currentLockOn;
    private float lookTime;

    private CinemachineFollow cinemachineFollow;
    private float targetOffset;

    private float offsetSmoothVelocity = 0f;
    #endregion

    #region Game Loop
    void Awake()
    {
        cameraState = CameraState.FreeAim;

        cinemachineFollow = lockOnCamera.GetComponent<CinemachineFollow>();
        targetOffset = lockOnOffsetMagnitude;
    }

    void LateUpdate()
    {
        switch (cameraState)
        {
            case CameraState.LockedOn:
            BreakLock();
            MoveLookAt();
            break;   
        }
    }
    #endregion

    #region Input Methods
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnLockOn(InputAction.CallbackContext context)
    {
        if (context.performed){
            LockOn();
        }    
    }
    #endregion

    #region Camera Methods
    void BreakLock()
    {
        if (lookInput.sqrMagnitude > 0.001f)
        {
            lookTime += Time.deltaTime;
        }        
        else lookTime = 0;

        if (lookTime > lockOnExitTime)
        {
            LockOn();
            lookTime = 0;
        }
    }

    void MoveLookAt()
    {
        if (currentLockOn != null){
            lockOnLookAt.position = currentLockOn.position;
        }

        if (playerController.moveInput.x > 0)
        {
            targetOffset = -1 * lockOnOffsetMagnitude;
        }
        else if (playerController.moveInput.x < 0)
        {
            targetOffset = lockOnOffsetMagnitude;
        }
        cinemachineFollow.FollowOffset.x = Mathf.SmoothDamp(cinemachineFollow.FollowOffset.x, targetOffset, ref offsetSmoothVelocity, offsetSmoothTime);
    }

    void LockOn()
    {
        switch (cameraState)
        {
            case CameraState.FreeAim:
                Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 100);
                float minDist = Mathf.Infinity;
                var currentCandidate = hitColliders[0];
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Lock On Point") && (hitCollider.transform.position - player.position).sqrMagnitude < minDist)
                    {
                        //must add angle check
                        minDist = (hitCollider.transform.position - player.position).sqrMagnitude;
                        currentCandidate = hitCollider;
                    }

                    currentLockOn = currentCandidate.GetComponent<Transform>();
                    lockOnCamera.SetActive(true);
                    cameraState = CameraState.LockedOn;
                }
                break;

            case CameraState.LockedOn:
                lockOnCamera.SetActive(false);
                cameraState = CameraState.FreeAim;
                break;
        }
    
    }
    #endregion
}
