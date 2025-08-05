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
    
    // Reference to pointing system for coordination
    private PointingSystem pointingSystem;

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
        
        // Find pointing system
        pointingSystem = GetComponent<PointingSystem>();
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
        
        bool droppedSomething = false;
        
        foreach (InteractableWand wand in heldWands)
        {
            if (wand.IsPickedUp())
            {
                wand.DropWand();
                droppedSomething = true;
                break; // Only drop one item at a time
            }
        }
        
        // If we didn't drop anything and player is trying to drop, maybe they want to point?
        if (!droppedSomething && pointingSystem != null && !pointingSystem.IsHandOut())
        {
            // Check if there are any wands to drop first
            bool hasWandEquipped = false;
            foreach (InteractableWand wand in heldWands)
            {
                if (wand.IsPickedUp())
                {
                    hasWandEquipped = true;
                    break;
                }
            }
            
            // If no wand equipped and trying to "drop", maybe they want to point?
            if (!hasWandEquipped)
            {
                Debug.Log("No wand to drop, did you mean to point? Use H key to take hand out.");
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