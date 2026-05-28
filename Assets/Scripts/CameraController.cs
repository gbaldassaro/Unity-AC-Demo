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

    #region Public Fields
    public GameObject orbit;
    public GameObject lockOn;
    public Transform player;
    public Transform orbitLookAt;
    public Transform lockOnLookAt;
    public Transform playerLookAt;

    [Range(0,100)]
    public float lockOnExitThreshold;

    [Header("Camera Smoothing")]
    public Vector3 lookAtSmoothVelocity = new Vector3(0,0,0);
    [Range(0,1)]
    public float lookAtSmoothTime;
    #endregion

    #region Private Fields
    private CameraState cameraState;
    private CinemachineInputAxisController orbitInput;
    private CinemachineCamera orbitCamera;
    private CinemachineOrbitalFollow orbitalFollow;
    private Vector2 lookInput;
    private Transform currentLockOn;
    #endregion

    #region Game Loop
    void Awake()
    {
        cameraState = CameraState.FreeAim;
        orbitInput = orbit.GetComponent<CinemachineInputAxisController>();
        orbitCamera = orbit.GetComponent<CinemachineCamera>();
        orbitalFollow = orbit.GetComponent<CinemachineOrbitalFollow>();
        currentLockOn = playerLookAt;
    }

    void LateUpdate()
    {
        MoveCamera();
        MoveLookAts();
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
    void MoveCamera()
    {
        switch (cameraState)
        {
            case CameraState.LockedOn:
                if (lookInput.sqrMagnitude > lockOnExitThreshold)
                {
                    //cameraState = CameraState.FreeAim;
                }
                break;
        }
        
    }

    void MoveLookAts()
    {
        orbitLookAt.position = playerLookAt.position;
        lockOnLookAt.position = Vector3.SmoothDamp(lockOnLookAt.position, currentLockOn.position, ref lookAtSmoothVelocity, lookAtSmoothTime);
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
                    orbitCamera.Priority = -1;
                    orbitInput.enabled = false;
                    orbitalFollow.enabled = false;
                    cameraState = CameraState.LockedOn;
                }
                break;

            case CameraState.LockedOn:
                orbitCamera.Priority = 1;
                orbitCamera.ForceCameraPosition(lockOn.transform.position, lockOn.transform.rotation);
                orbitInput.enabled = true;
                orbitalFollow.enabled = true;
                cameraState = CameraState.FreeAim;
                break;
        }
        
    }
    #endregion
}
