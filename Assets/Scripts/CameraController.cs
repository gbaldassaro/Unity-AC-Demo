using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraState
{
    LockedOn,
    FreeAim
}

public class CameraController : MonoBehaviour
{

    public GameObject player;
    public Transform cameraFollowPoint;
    [Range(2,20)]
    public float distanceFromPlayer;
    [Range(0,90)]
    public float verticalCamAngleLimit;
    [Range(0,2)]
    public float sensitivity;
    [Range(0,50)]
    public float lockOnExitThreshold;

    private CameraState cameraState;

    private Vector2 lookInput;

    void Start()
    {
        cameraState = CameraState.FreeAim;
    }

    void LateUpdate()
    {
        MoveCamera();
        PointCamera();
        PushCamera();
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

                    Vector3 axisVertical = -transform.right;
                    Vector3 axisHorizontal = transform.up;

                    transform.RotateAround(player.transform.position, axisHorizontal.normalized, speed.x);

                    GameObject temp = new GameObject();
                    temp.transform.position = transform.position;
                    temp.transform.rotation = transform.rotation;

                    temp.transform.RotateAround(player.transform.position, axisVertical.normalized, speed.y);
                    Vector3 offset = temp.transform.position - player.transform.position;
                    Vector3 positivePosition = new Vector3(offset.x, Math.Abs(offset.y), offset.z);
                    if (Math.Abs(Vector3.Angle(positivePosition.normalized, Vector3.up)) > verticalCamAngleLimit)
                    {
                        transform.RotateAround(player.transform.position, axisVertical.normalized, speed.y);
                    }
                    
                    Destroy(temp);

                }
                break;
        }
    }

    void PointCamera()
    {
        transform.forward = cameraFollowPoint.position - transform.position;
    }

    void PushCamera()
    {
        Vector3 offset = transform.position - player.transform.position;
        transform.position += transform.forward * (offset.magnitude - distanceFromPlayer);
    }
}
