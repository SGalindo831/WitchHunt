using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 5f;
    [SerializeField] private LayerMask interactableLayerMask = -1;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.G;
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CrosshairUI crosshairUI;
    
    private IInteractable currentInteractable;
    private IInteractable lastInteractable;

    private void Start()
    {
        // Find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Find crosshair UI if not assigned
        if (crosshairUI == null)
        {
            crosshairUI = FindFirstObjectByType<CrosshairUI>();
        }
    }

    private void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
    }

    private void CheckForInteractable()
    {
        // Cast ray from center of screen
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;
        
        currentInteractable = null;
        
        // Check if ray hits an interactable object
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayerMask))
        {
            // Try to get IInteractable component
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null && interactable.CanInteract())
            {
                currentInteractable = interactable;
            }
        }
        
        // Handle hover enter/exit
        if (currentInteractable != lastInteractable)
        {
            // Exit hover on last object
            if (lastInteractable != null)
            {
                lastInteractable.OnHoverExit();
                SetCrosshairHighlight(false);
            }
            
            // Enter hover on new object
            if (currentInteractable != null)
            {
                currentInteractable.OnHoverEnter();
                SetCrosshairHighlight(true);
            }
            
            lastInteractable = currentInteractable;
        }
    }

    private void HandleInteractionInput()
    {
        // Check for pickup interaction
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            currentInteractable.OnInteract(transform);
        }
        
        // Check for drop input - works on any held item
        if (Input.GetKeyDown(dropKey))
        {
            HandleDropInput();
        }
    }
    
    private void HandleDropInput()
    {
        // Find any held interactable objects and drop them
        InteractableWand[] heldWands = GetComponentsInChildren<InteractableWand>();
        
        foreach (InteractableWand wand in heldWands)
        {
            if (wand.IsPickedUp())
            {
                wand.DropWand(); // We'll need to make this method public
                break; // Only drop one item at a time
            }
        }
    }

    private void SetCrosshairHighlight(bool highlighted)
    {
        if (crosshairUI != null)
        {
            crosshairUI.SetCrosshairHighlight(highlighted);
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Vector3 rayStart = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            Gizmos.DrawRay(rayStart, rayDirection * interactionRange);
        }
    }

    // Public methods for getting current interaction info
    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }

    public bool HasInteractable()
    {
        return currentInteractable != null;
    }

    public string GetCurrentInteractionPrompt()
    {
        return currentInteractable?.GetInteractionPrompt() ?? "";
    }
}