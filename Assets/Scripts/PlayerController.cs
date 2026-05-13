using System;
using UnityEngine;
using UnityEngine.InputSystem;

#region Enums
public enum PlayerState
{
    Idle,
    Moving,
    Boosting,
    Flying
}

public enum CameraState
{
    LockedOn,
    FreeAim
}
#endregion

public class PlayerController : MonoBehaviour
{
    #region Public Fields
    [Header("Camera")]
    public Camera mainCamera;
    public Transform cameraFollowPoint;
    [Range(0,20)]
    public float distanceFromPlayer;
    [Range(0,90)]
    public float verticalCamAngleLimit;
    [Range(0,2)]
    public float sensitivity;
    [Range(0,50)]
    public float lockOnExitThreshold;

    [Header("Player")]
    [Range(0,100)]
    public float jumpHeight;
    [Range(0,10)]
    public float moveSpeed;
    [Range(0,10)]
    public float gravity;
    #endregion
    
    #region Private Fields
    private CharacterController characterController;
    private PlayerState playerState;
    private CameraState cameraState;

    private Vector3 moveDirection;
    #endregion

    #region Input Fields
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool lookHeld;
    #endregion

    #region Game Loop
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerState = PlayerState.Idle;
        cameraState = CameraState.FreeAim;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        MovePlayer();
    }

    void LateUpdate()
    {
        MoveCamera();
        PointCamera();
        PushCamera();
    }
    #endregion

    #region Input Methods
    void OnJump()
    {
        if (characterController.isGrounded)
        {
            Debug.Log("hello");
        }
    }

    void OnMove(InputValue value) 
    {
        moveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void OnLockOn()
    {
        switch (cameraState)
        {
            case CameraState.FreeAim:
                cameraState = CameraState.LockedOn;
                break;
            case CameraState.LockedOn:
                cameraState = CameraState.FreeAim;
                break;
        }
    }
    #endregion

    #region Player Methods
    void MovePlayer()
    {
        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0;
        Vector3 right = mainCamera.transform.right;
        right.y = 0;
        moveDirection = (forward.normalized * moveInput.y) + (right.normalized * moveInput.x);

        characterController.SimpleMove(moveDirection * moveSpeed);
    }
    #endregion

    #region Camera Methods
    void MoveCamera()
    {
        Vector2 speed = sensitivity * lookInput;

        switch (cameraState)
        {
            case CameraState.LockedOn:
                if (speed.sqrMagnitude > lockOnExitThreshold)
                {
                    cameraState = CameraState.FreeAim;
                }
                break;
            
            case CameraState.FreeAim:
                if (speed.sqrMagnitude > 0.001f){

                    Vector3 axisVertical = -mainCamera.transform.right;
                    Vector3 axisHorizontal = mainCamera.transform.up;

                    mainCamera.transform.RotateAround(transform.position, axisHorizontal.normalized, speed.x);

                    GameObject temp = new GameObject();
                    temp.transform.position = mainCamera.transform.position;
                    temp.transform.rotation = mainCamera.transform.rotation;

                    temp.transform.RotateAround(transform.position, axisVertical.normalized, speed.y);
                    Vector3 offset = temp.transform.position - transform.position;
                    Vector3 positivePosition = new Vector3(offset.x, Math.Abs(offset.y), offset.z);
                    if (Math.Abs(Vector3.Angle(positivePosition.normalized, Vector3.up)) > verticalCamAngleLimit)
                    {
                        mainCamera.transform.RotateAround(transform.position, axisVertical.normalized, speed.y);
                    }
                    
                    Destroy(temp);

                }
                break;
        }
    }

    void PointCamera()
    {
        mainCamera.transform.forward = cameraFollowPoint.position - mainCamera.transform.position;
    }

    void PushCamera()
    {
        Vector3 offset = mainCamera.transform.position - transform.position;
        mainCamera.transform.position += mainCamera.transform.forward * (offset.magnitude - distanceFromPlayer);
    }
    #endregion
}
