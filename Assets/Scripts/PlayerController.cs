using System;
using UnityEngine;
using UnityEngine.InputSystem;

#region Enums
public enum PlayerState
{
    Idle,
    Walking,
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
    #endregion
    
    #region Private Fields
    private Rigidbody rb;
    private PlayerState playerState;
    private CameraState cameraState;
    #endregion

    #region Input Fields
    private bool moveHeld;
    private Vector2 moveInput;
    private bool lookHeld;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerState = PlayerState.Idle;
        cameraState = CameraState.FreeAim;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        Debug.Log(moveHeld);
        if (moveHeld)
        {
            Vector2 speed = moveSpeed * moveInput;
            rb.linearVelocity = new Vector3(speed.x, 0, speed.y);
        }
    }

    void LateUpdate()
    {
        PointCamera(cameraFollowPoint);
        PushCamera(distanceFromPlayer);
    }

    #region Input Methods
    void OnJump()
    {
        rb.AddForce(0,jumpHeight,0);
    }

    void OnMove(InputValue value) 
    {
        moveHeld = !moveHeld;
        moveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        Vector2 lookInput = value.Get<Vector2>();
        Vector2 speed = sensitivity * lookInput;

        switch (cameraState)
        {
            case CameraState.LockedOn:
                if (speed.magnitude > lockOnExitThreshold)
                {
                    cameraState = CameraState.FreeAim;
                }
                break;
            
            case CameraState.FreeAim:
                Vector3 perp = Vector3.Cross(mainCamera.transform.forward, mainCamera.transform.up).normalized;
                Vector3 direction = (mainCamera.transform.up * speed.x) + (perp * speed.y);
                Vector3 temp = mainCamera.transform.position;
                mainCamera.transform.RotateAround(transform.position, direction, speed.magnitude);
                // Vector3 positivePosition = new Vector3(mainCamera.transform.position.x, Math.Abs(mainCamera.transform.position.y), mainCamera.transform.position.z);
                // Debug.Log(Math.Abs(Vector3.Dot(positivePosition.normalized, Vector3.up)));
                // if (Math.Abs(Vector3.Dot(positivePosition.normalized, Vector3.up)) > Math.Cos(verticalCamAngleLimit*Math.PI/180))
                // {
                //     mainCamera.transform.position = temp;
                // }
                break;
        }
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

    #region Camera Methods
    void PointCamera(Transform cameraFollowPoint)
    {
        mainCamera.transform.forward = cameraFollowPoint.position - mainCamera.transform.position;
    }

    void PushCamera(float distanceFromPlayer)
    {
        Vector3 offset = mainCamera.transform.position - transform.position;
        mainCamera.transform.position += mainCamera.transform.forward * (offset.magnitude - distanceFromPlayer);
    }
    #endregion
}
