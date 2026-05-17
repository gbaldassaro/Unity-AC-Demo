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
    public Transform cameraLookAt;
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
    private Vector2 lookInput;
    private Transform currentLookAt;
    #endregion

    #region Game Loop
    void Awake()
    {
        cameraState = CameraState.FreeAim;
        orbitInput = orbit.GetComponent<CinemachineInputAxisController>();
        orbitCamera = orbit.GetComponent<CinemachineCamera>();
        currentLookAt = playerLookAt;
    }

    void LateUpdate()
    {
        MoveCamera();
        MoveCameraLookAt();
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

    void MoveCameraLookAt()
    {
        cameraLookAt.position = Vector3.SmoothDamp(cameraLookAt.position, currentLookAt.position, ref lookAtSmoothVelocity, lookAtSmoothTime);
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

                    currentLookAt = currentCandidate.GetComponent<Transform>();
                    cameraState = CameraState.LockedOn;
                    orbitCamera.Priority = -1;
                    orbitInput.enabled = false;
                }
                break;

            case CameraState.LockedOn:
                currentLookAt = playerLookAt;
                cameraState = CameraState.FreeAim;
                orbitCamera.Priority = 1;
                orbitInput.enabled = true;
                orbit.transform.position = lockOn.transform.position;
                orbit.transform.rotation = lockOn.transform.rotation;
                break;
        }
        
    }
    #endregion
}
