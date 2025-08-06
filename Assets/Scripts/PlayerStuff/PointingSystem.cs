using Unity.VisualScripting;
using UnityEngine;

public class PointingSystem : MonoBehaviour
{
    [Header("Pointing Settings")]
    [SerializeField] private float pointingRange = 10f;
    [SerializeField] private LayerMask playerLayerMask = -1;
    [SerializeField] private KeyCode pointKey = KeyCode.F; // Key to point
    [SerializeField] private KeyCode putAwayKey = KeyCode.H; // Key to put hand away
    
    [Header("Hand References")]
    [SerializeField] private GameObject handGameObject; // Your hand GameObject with arm and finger
    [SerializeField] private Transform armTransform; // The arm child object
    [SerializeField] private Transform fingerTransform; // The finger child object
    [SerializeField] private Transform handAttachPoint; // Where the hand attaches to player
    
    [Header("Animation Settings")]
    [SerializeField] private float pointingSpeed = 2f;
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CrosshairUI crosshairUI;
    
    // States
    private bool isHandOut = false;
    private bool isPointing = false;
    
    // Pointing target
    private Transform currentTarget;

    // Wand System integration
    private InteractableWand equippedWand;

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

        // Find hand attach point if not assigned (look for HandPosition like your wand system)
        if (handAttachPoint == null)
        {
            handAttachPoint = FindHandAttachPoint();
        }

        // Set up hand GameObject
        SetupHandGameObject();

        // Start with hand put away
        PutHandAway();
    }

    private void Update()
    {
        UpdateEquippedWandReference();
        HandleInput();
        
        if (isHandOut)
        {
            UpdatePointing();
        }
    }

    private void UpdateEquippedWandReference()
    {
        InteractableWand[] wands = GetComponentsInChildren<InteractableWand>();
        equippedWand = null;

        foreach (InteractableWand wand in wands)
        {
            if (wand.IsPickedUp())
            {
                equippedWand = wand;
                break;
            }
        }
    }

    private void AttemptToTakeHandOut()
    {
        //We check if we have the wand equipped
        if (HasWandEquipped())
        {
            Debug.Log("Cannot point while holding wand. Drop wand first!");
            return;
        }
        
        // If no wand equipped, take hand out
        TakeHandOut();
    }

    private void ForceDropWandAndPoint()
    {
        //This will be called when the player wants to force point and drop the wand
        if (HasWandEquipped())
        {
            Debug.Log("Dropping wand to point");
            equippedWand.DropWand();
            //Adding small delay so hand wont instantly clip into wand
            Invoke(nameof(TakeHandOut), 0.1f);
        }
        else
        {
            TakeHandOut();
        }
    }
    

    private void HandleInput()
    {
        // Toggle hand out/away
        if (Input.GetKeyDown(putAwayKey))
        {
            if (isHandOut)
            {
                PutHandAway();
            }
            else
            {
                AttemptToTakeHandOut();
            }
        }

        // Point at target (only if hand is out)
        if (Input.GetKeyDown(pointKey) && isHandOut)
        {
            AttemptToPoint();
        }
    }

    private void UpdatePointing()
    {
        // Cast ray from center of screen to find pointing target
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;
        
        Transform newTarget = null;
        
        // Check if ray hits a player
        if (Physics.Raycast(ray, out hit, pointingRange, playerLayerMask))
        {
            // Check if hit object is a player
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Human"))
            {
                // Make sure we're not pointing at ourselves
                if (hit.collider.transform.root != transform.root)
                {
                    newTarget = hit.collider.transform;
                }
            }
        }
        
        // Update target
        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            UpdateHandDirection();
        }
    }

    private void UpdateHandDirection()
    {
        if (handGameObject == null || !isHandOut) return;
        
        if (currentTarget != null)
        {
            // Point at the target
            Vector3 direction = (currentTarget.position - handGameObject.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Apply rotation to the arm
            if (armTransform != null)
            {
                armTransform.rotation = Quaternion.Slerp(armTransform.rotation, targetRotation, Time.deltaTime * pointingSpeed);
            }
            
            // Highlight crosshair when pointing at a player
            SetCrosshairHighlight(true);
        }
        else
        {
            // Return to default pointing position (straight ahead)
            Vector3 defaultDirection = playerCamera.transform.forward;
            Quaternion targetRotation = Quaternion.LookRotation(defaultDirection);
            
            if (armTransform != null)
            {
                armTransform.rotation = Quaternion.Slerp(armTransform.rotation, targetRotation, Time.deltaTime * pointingSpeed);
            }
            
            SetCrosshairHighlight(false);
        }
    }

    private void AttemptToPoint()
    {
        if (currentTarget != null)
        {
            // Found a target to point at!
            Debug.Log($"Pointing at: {currentTarget.name}");
            
            CallOutWitch(currentTarget);
        }
        else
        {
            Debug.Log("No valid target to point at!");
        }
    }

    private void CallOutWitch(Transform target)
    {
        Debug.Log($"WITCH! Accusing {target.name}!");
        
        // TODO: Add trial scene transition here
        // For now, just show a message
        
        // TODO:
        // 1. Freeze all players
        // 2. Show accusation UI
        // 3. Transition to trial scene
        // 4. Pass the accused player data to the trial system
        
        // TrialManager.Instance.StartTrial(transform, target);
        // SceneManager.LoadScene("TrialScene");
    }

    public void TakeHandOut()
    {
        // Double check that we don't have a wand equipped
        if (HasWandEquipped())
        {
            Debug.Log("Cannot take hand out while wand is equipped!");
            return;
        }
        
        if (isHandOut) return;
        
        Debug.Log("Taking hand out");
        isHandOut = true;
        
        if (handGameObject != null)
        {
            handGameObject.SetActive(true);
        }
    }
    
    public void PutHandAway()
    {
        Debug.Log("Putting hand away");
        isHandOut = false;
        isPointing = false;
        currentTarget = null;
        
        if (handGameObject != null)
        {
            handGameObject.SetActive(false);
        }
        
        SetCrosshairHighlight(false);
    }
    
    // Method to automatically put hand away when picking up wand
    public void OnWandPickedUp()
    {
        if (isHandOut)
        {
            Debug.Log("Automatically putting hand away because wand was picked up");
            PutHandAway();
        }
    }

    private void SetupHandGameObject()
    {
        if (handGameObject == null)
        {
            Debug.LogWarning("Hand GameObject not assigned in PointingSystem!");
            return;
        }

        // Attach hand to the player if it's not already
        if (handAttachPoint != null)
        {
            handGameObject.transform.SetParent(handAttachPoint);
        }
        else
        {
            // Fallback: attach to player directly
            handGameObject.transform.SetParent(transform);
        }

        // Find arm and finger transforms if not assigned
        if (armTransform == null)
        {
            armTransform = handGameObject.transform.Find("Arm");
        }

        if (fingerTransform == null)
        {
            fingerTransform = handGameObject.transform.Find("Finger");
        }

        // Log what we found
        Debug.Log($"Hand setup complete. Arm: {(armTransform != null ? "Found" : "Not found")}, Finger: {(fingerTransform != null ? "Found" : "Not found")}");
    }

    private Transform FindHandAttachPoint()
    {
        // Look for hand attachment points
        Transform handPoint = transform.Find("RightHandPosition");
        if (handPoint != null) return handPoint;
        
        handPoint = transform.Find("HandPosition");
        if (handPoint != null) return handPoint;
        
        handPoint = transform.Find("RightHand");
        if (handPoint != null) return handPoint;
        
        handPoint = transform.Find("Hand");
        if (handPoint != null) return handPoint;
        
        Debug.LogWarning("No hand attach point found. Hand will attach to player root.");
        return transform;
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
        if (playerCamera != null && isHandOut)
        {
            Gizmos.color = Color.blue;
            Vector3 rayStart = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            Gizmos.DrawRay(rayStart, rayDirection * pointingRange);
            
            // Draw target indicator
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
            }
        }
    }

    private bool HasWandEquipped()
    {
        return equippedWand != null && equippedWand.IsPickedUp();
    }

    // Public methods for external systems
    public bool IsHandOut() => isHandOut;
    public bool IsPointing() => isPointing;
    public Transform GetCurrentTarget() => currentTarget;
    public bool CanPoint() => isHandOut && currentTarget != null && !HasWandEquipped();
}