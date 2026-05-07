using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;
    public Transform cameraFollowPoint;
    [Range(0,20)]
    public float distanceFromPlayer;
    [Range(0,2)]
    public float sensitivity;

    [Header("Player")]
    [Range(0,100)]
    public float jumpHeight;

    [Range(0,10)]
    public float moveSpeed;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        mainCamera.transform.forward = cameraFollowPoint.position - mainCamera.transform.position;
    }

    void OnJump()
    {
        rb.AddForce(0,jumpHeight,0);
    }

    void OnMove(InputValue value) 
    {

    }

    void OnLook(InputValue value)
    {
        Vector2 lookInput = value.Get<Vector2>();
        Vector2 speed = sensitivity * lookInput;
        Debug.Log(speed);
        Vector3 perp = Vector3.Cross(mainCamera.transform.forward, mainCamera.transform.up).normalized;
        mainCamera.transform.position += (perp * speed.x) - (mainCamera.transform.up * speed.y);
        Vector3 offset = mainCamera.transform.position - cameraFollowPoint.position;
        mainCamera.transform.position += mainCamera.transform.forward * (offset.magnitude - distanceFromPlayer);
    }
}
