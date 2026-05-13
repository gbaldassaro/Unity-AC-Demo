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
    public float moveSpeed;
    [Range(0,10)]
    public float gravity;
    #endregion
    
    #region Private Fields
    private CharacterController characterController;
    private PlayerState playerState;

    private Vector3 moveDirection;
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
}
