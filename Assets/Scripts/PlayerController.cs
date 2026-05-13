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
#endregion

public class PlayerController : MonoBehaviour
{
    #region Public Fields
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Player")]
    [Range(0,100)]
    public float jumpHeight;
    [Range(0,10)]
    public float walkMaxSpeed;
    [Range(0,10)]
    public float walkAcceleration;
    [Range(0,10)]
    public float boostMaxSpeed;
    [Range(0,10)]
    public float boostAcceleration;
    [Range(0,10)]
    public float friction;
    [Range(0,10)]
    public float gravity;
    #endregion
    
    #region Private Fields
    private CharacterController characterController;
    private PlayerState playerState;

    private Vector3 moveDirection;
    private Vector3 accelerationDirection;
    #endregion

    #region Input Fields
    private Vector2 moveInput;
    #endregion

    #region Game Loop
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerState = PlayerState.Idle;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        MovePlayer();
        Debug.Log(playerState);
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
        switch (playerState)
        {
            case PlayerState.Idle:
                playerState = PlayerState.Moving;
                break;
        }
    }

    void OnBoost()
    {
        switch (playerState)
        {
            case PlayerState.Moving:
                playerState = PlayerState.Boosting;
                break;
            case PlayerState.Boosting:
                playerState = PlayerState.Moving;
                break;
        }
    }
    #endregion

    #region Player Methods
    void MovePlayer()
    {
        if (playerState != PlayerState.Idle)
        {
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0;
            Vector3 right = mainCamera.transform.right;
            right.y = 0;
            accelerationDirection = (forward.normalized * moveInput.y) + (right.normalized * moveInput.x);

            float maxSpeed = 0; 

            switch (playerState)
            {
                case PlayerState.Moving:
                    maxSpeed = walkMaxSpeed;
                    accelerationDirection *= walkAcceleration;
                    break;
                case PlayerState.Boosting:
                    maxSpeed = boostMaxSpeed;
                    accelerationDirection *= boostAcceleration;
                    break;
            }

            if (moveInput.sqrMagnitude < 0.001f)
            {
                accelerationDirection = -moveDirection * friction;
            }

            moveDirection += accelerationDirection;

            if (moveDirection.magnitude > maxSpeed)
            {
                moveDirection = moveDirection.normalized * maxSpeed;
            }

            if (moveDirection.magnitude < 0.01f)
            {
                playerState = PlayerState.Idle;
            }

            characterController.Move(moveDirection * Time.deltaTime);
        }
    }
    #endregion
}
