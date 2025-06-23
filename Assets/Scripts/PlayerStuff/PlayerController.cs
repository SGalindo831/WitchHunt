using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    
    [Header("Camera")]
    [SerializeField] private Transform followTarget; // The empty object Cinemachine follows
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    private float mouseX;
    private float mouseY;
    private float verticalRotation = 0f;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float jumpHeight = 2f;

    private float verticalVelocity;

    [Header("Input")]
    private float moveInput;
    private float turnInput;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        InputManagement();
        CameraMovement();
        Movement();
    }

    private void CameraMovement()
    {
        // Get mouse input
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate player horizontally
        transform.Rotate(0, mouseX, 0);
        
        // Handle vertical rotation of the follow target (what Cinemachine looks at)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        // Apply vertical rotation to the follow target
        if (followTarget != null)
        {
            followTarget.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    private void Movement()
    {
        GroundMovement();
    }

    private void GroundMovement()
    {
        // Move relative to player's current rotation
        Vector3 move = transform.right * turnInput + transform.forward * moveInput;
        
        move *= walkSpeed;
        move.y = VerticalForceCalculation();

        controller.Move(move * Time.deltaTime);
    }

    private float VerticalForceCalculation()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    private void InputManagement()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        
        // Allow player to unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}