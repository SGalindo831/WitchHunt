// using UnityEngine;

// public class PlayerController : MonoBehaviour
// {
//     [Header("Refrences")]
//     private CharacterController controller;

//     [Header("Movement Settings")]
//     [SerializeField] private float walkSpeed = 5f;
//     [SerializeField] private float gravity = 9.8f;
//     [SerializeField] private float jumpHeight = 2f;

//     private float verticalVelocity;

//     [Header("Input")]
//     private float moveInput;
//     private float turnInput;

//     private void Start()
//     {
//         controller = GetComponent<CharacterController>();
//     }

//     private void Update()
//     {
//         InputManagement();
//         Movement();
//     }

//     private void Movement()
//     {
//         GroundMovement();
//     }

//     private void GroundMovement()
//     {
//         Vector3 move = new Vector3(turnInput, 0, moveInput);

//         move *= walkSpeed;

//         move.y = VerticalForceCalculation();

//         controller.Move(move * Time.deltaTime);
//     }

//     private float VerticalForceCalculation()
//     {
//         if (controller.isGrounded)
//         {
//             verticalVelocity = -1f;
//             if (Input.GetButtonDown("Jump"))
//             {
//                 verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
//             }
//         }
//         else
//         {
//             verticalVelocity -= gravity * Time.deltaTime;
//         }
//         return verticalVelocity;
//     }

//     private void InputManagement()
//     {
//         moveInput = Input.GetAxis("Vertical");
//         turnInput = Input.GetAxis("Horizontal");
//     }
// }

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform; // Drag your Main Camera here
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
        
        // If camera transform not assigned, try to find it
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
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
        
        // Rotate player horizontally (this rotates the player with the camera)
        transform.Rotate(0, mouseX, 0);
        
        // Handle vertical camera rotation
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        
        // Apply vertical rotation to camera only
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    private void Movement()
    {
        GroundMovement();
    }

    private void GroundMovement()
    {
        // Move relative to player's current rotation (which follows camera)
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