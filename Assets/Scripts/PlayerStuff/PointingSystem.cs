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
        HandleInput();
        
        if (isHandOut)
        {
            UpdatePointing();
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
                TakeHandOut();
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
            // Check if hit object is a player (you might want to adjust this based on your player tags)
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
            
            // Apply rotation to the arm (you might need to adjust the axis based on your model)
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
            
            // Here you can add the "Witch!" callout functionality
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
        
        // TODO: Add your trial scene transition here
        // For now, just show a message
        
        // You might want to:
        // 1. Freeze all players
        // 2. Show accusation UI
        // 3. Transition to trial scene
        // 4. Pass the accused player data to the trial system
        
        // Example of what you might do:
        // TrialManager.Instance.StartTrial(transform, target);
        // SceneManager.LoadScene("TrialScene");
    }

    public void TakeHandOut()
    {
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
        // Look for hand attachment points (similar to your wand system)
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

    // Public methods for external systems
    public bool IsHandOut() => isHandOut;
    public bool IsPointing() => isPointing;
    public Transform GetCurrentTarget() => currentTarget;
    public bool CanPoint() => isHandOut && currentTarget != null;
}