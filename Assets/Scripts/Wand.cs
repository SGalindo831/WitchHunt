using UnityEngine;

public class InteractableWand : MonoBehaviour, IInteractable
{
    [Header("Wand Settings")]
    [SerializeField] private string wandName = "Magic Wand";
    [SerializeField] private bool isPickedUp = false;
    
    [Header("Visual Effects")]
    [SerializeField] private Color highlightColor = Color.yellow;
    private Color originalColor; // Store the original color instead of assuming white
    
    private Renderer wandRenderer;
    private Rigidbody wandRigidbody;
    private Collider wandCollider;
    private Transform originalParent;
    private bool isHighlighted = false;

    private void Start()
    {
        // Get components
        wandRenderer = GetComponent<Renderer>();
        wandRigidbody = GetComponent<Rigidbody>();
        wandCollider = GetComponent<Collider>();
        originalParent = transform.parent;
        
        // Store the original color of the wand
        if (wandRenderer != null && wandRenderer.material != null)
        {
            originalColor = wandRenderer.material.color;
            Debug.Log($"Stored original wand color: {originalColor}");
        }
        else
        {
            originalColor = Color.white; // Fallback
        }
        
        // Make sure the wand has required components
        if (wandRenderer == null)
        {
            Debug.LogError($"Wand {wandName} needs a Renderer component!");
        }
        
        if (wandRigidbody == null)
        {
            wandRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        if (wandCollider == null)
        {
            Debug.LogError($"Wand {wandName} needs a Collider component!");
        }
    }

    // Interface method - called when player looks at the wand
    public void OnHoverEnter()
    {
        if (!isPickedUp && !isHighlighted)
        {
            SetHighlight(true);
        }
    }

    // Interface method - called when player stops looking at the wand
    public void OnHoverExit()
    {
        if (!isPickedUp && isHighlighted)
        {
            SetHighlight(false);
        }
    }

    // Interface method - called when player interacts with the wand
    public void OnInteract(Transform player)
    {
        if (!isPickedUp)
        {
            PickUpWand(player);
        }
        // Remove the else clause - dropping is now handled by G key
    }

    // Interface method - returns interaction prompt
    public string GetInteractionPrompt()
    {
        return isPickedUp ? "" : $"Pick up {wandName}"; // No prompt when picked up
    }

    // Interface method - checks if this object can be interacted with
    public bool CanInteract()
    {
        return true; // Wand can always be interacted with
    }

    private void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        if (wandRenderer != null)
        {
            // Use original color instead of assuming white
            wandRenderer.material.color = highlight ? highlightColor : originalColor;
        }
    }

    private void PickUpWand(Transform player)
    {
        Debug.Log($"Picked up {wandName}!");
        
        isPickedUp = true;
        SetHighlight(false);
        
        // Disable physics
        if (wandRigidbody != null)
        {
            wandRigidbody.isKinematic = true;
        }
        
        // Disable collider (so it doesn't interfere with player movement)
        if (wandCollider != null)
        {
            wandCollider.enabled = false;
        }
        
        // Attach to player's hand (you can create a hand transform later)
        Transform playerHand = FindPlayerHand(player);
        if (playerHand != null)
        {
            transform.SetParent(playerHand);
            transform.localPosition = Vector3.zero;
            
            // Set wand to pointing position (90 degrees forward)
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Points straight forward
            
            Debug.Log($"Wand attached to hand at: {playerHand.position}");
        }
        else
        {
            // Fallback: attach to player
            transform.SetParent(player);
            transform.localPosition = new Vector3(0.5f, 0.5f, 1f); // Rough hand position
            
            // Set pointing position for fallback too
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Points straight forward
            
            Debug.Log("No hand found, attached to player directly");
        }
    }

    private void DropWandInternal()
    {
        Debug.Log($"Dropped {wandName}!");
        
        isPickedUp = false;
        
        // Get player position and forward direction before detaching
        Transform player = transform.parent;
        
        // DEBUG: Check what the parent actually is
        Debug.Log($"Wand parent name: {(player != null ? player.name : "NULL")}");
        Debug.Log($"Wand parent position: {(player != null ? player.position : Vector3.zero)}");
        
        // Try to find the actual player controller
        PlayerController actualPlayer = FindFirstObjectByType<PlayerController>();
        if (actualPlayer != null)
        {
            Debug.Log($"Actual player name: {actualPlayer.name}");
            Debug.Log($"Actual player position: {actualPlayer.transform.position}");
            
            // Use the actual player position instead
            Vector3 playerPosition = actualPlayer.transform.position;
            Vector3 playerForward = actualPlayer.transform.forward;
            
            // Detach from current parent
            transform.SetParent(originalParent);
            
            // Drop at actual player position
            Vector3 dropPosition = playerPosition + playerForward * 2f + Vector3.up * 1f;
            transform.position = dropPosition;
            
            Debug.Log($"Wand dropped at: {dropPosition}");
        }
        else
        {
            Debug.LogError("Could not find PlayerController!");
            return;
        }
        
        // Re-enable physics
        if (wandRigidbody != null)
        {
            wandRigidbody.isKinematic = false;
        }
        
        // Re-enable collider
        if (wandCollider != null)
        {
            wandCollider.enabled = true;
        }
        
        // Add a small forward force when dropping
        if (wandRigidbody != null)
        {
            Vector3 playerForward = FindFirstObjectByType<PlayerController>().transform.forward;
            wandRigidbody.AddForce(playerForward * 3f + Vector3.up * 2f, ForceMode.Impulse);
        }
        
        Debug.Log($"Wand final position: {transform.position}");
        Debug.Log($"Wand active: {gameObject.activeInHierarchy}");
        Debug.Log($"Wand renderer enabled: {wandRenderer != null && wandRenderer.enabled}");
    }
    
    // Public method to drop the wand (called by InteractionManager)
    public void DropWand()
    {
        if (isPickedUp)
        {
            DropWandInternal();
        }
    }

    private Transform FindPlayerHand(Transform player)
    {
        Debug.Log($"Looking for hand on player: {player.name}");
        
        // Look for hand transforms in the player hierarchy
        Transform hand = player.Find("HandPosition");  // Added this one first!
        if (hand != null) 
        {
            Debug.Log("Found HandPosition!");
            return hand;
        }
        
        hand = player.Find("PlayerHand");
        if (hand != null) 
        {
            Debug.Log("Found PlayerHand!");
            return hand;
        }
        
        hand = player.Find("Hand");
        if (hand != null) 
        {
            Debug.Log("Found Hand!");
            return hand;
        }
        
        hand = player.Find("RightHand");
        if (hand != null) 
        {
            Debug.Log("Found RightHand!");
            return hand;
        }
        
        Debug.Log("No hand found! Available children:");
        for (int i = 0; i < player.childCount; i++)
        {
            Debug.Log($"Child {i}: {player.GetChild(i).name}");
        }
        
        return null; // Will return null if not found
    }

    // Public method to check if wand is currently picked up
    public bool IsPickedUp()
    {
        return isPickedUp;
    }

    // Public method to get wand name
    public string GetWandName()
    {
        return wandName;
    }
}