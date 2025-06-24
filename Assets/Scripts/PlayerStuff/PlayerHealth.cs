// PlayerHealth.cs - Add this to all players
using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player State")]
    public bool isHuman = true;
    public bool isFrog = false;
    
    [Header("Frog Transformation")]
    [SerializeField] private GameObject frogModel; // Assign frog prefab/model
    [SerializeField] private GameObject humanModel; // Assign human model
    [SerializeField] private float frogDuration = 15f; // How long they stay a frog
    
    [Header("Movement Changes")]
    [SerializeField] private float frogMoveSpeed = 2f;
    [SerializeField] private float normalMoveSpeed = 5f;
    
    private PlayerController playerController;
    private WandProjectile wandProjectile;
    private Coroutine frogTransformCoroutine;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        wandProjectile = GetComponent<WandProjectile>();
        
        // Make sure we start as human
        SetHumanForm();
    }

    public void TransformToFrog()
    {
        if (isFrog) return; // Already a frog
        
        Debug.Log($"{gameObject.name} has been turned into a frog!");
        
        isFrog = true;
        isHuman = false;
        
        // Visual changes
        if (humanModel != null) humanModel.SetActive(false);
        if (frogModel != null) frogModel.SetActive(true);
        
        // Disable wand usage
        if (wandProjectile != null)
        {
            wandProjectile.enabled = false;
        }
        
        // Reduce movement speed
        if (playerController != null)
        {
            // You'll need to make walkSpeed public in PlayerController or add a setter method
            // playerController.walkSpeed = frogMoveSpeed;
        }
        
        // Start transformation timer
        if (frogTransformCoroutine != null)
        {
            StopCoroutine(frogTransformCoroutine);
        }
        frogTransformCoroutine = StartCoroutine(FrogTransformationTimer());
    }
    
    private IEnumerator FrogTransformationTimer()
    {
        yield return new WaitForSeconds(frogDuration);
        TransformBackToHuman();
    }
    
    public void TransformBackToHuman()
    {
        if (!isFrog) return; // Not a frog
        
        Debug.Log($"{gameObject.name} has transformed back to human!");
        
        SetHumanForm();
    }
    
    private void SetHumanForm()
    {
        isFrog = false;
        isHuman = true;
        
        // Visual changes
        if (frogModel != null) frogModel.SetActive(false);
        if (humanModel != null) humanModel.SetActive(true);
        
        // Re-enable wand usage
        if (wandProjectile != null)
        {
            wandProjectile.enabled = true;
        }
        
        // Restore normal movement speed
        if (playerController != null)
        {
            // playerController.walkSpeed = normalMoveSpeed;
        }
    }
    
    // Public getters
    public bool IsHuman() => isHuman;
    public bool IsFrog() => isFrog;
    
    // Method to instantly cure frog transformation (for power-ups, etc.)
    public void CureFrogTransformation()
    {
        if (frogTransformCoroutine != null)
        {
            StopCoroutine(frogTransformCoroutine);
            frogTransformCoroutine = null;
        }
        TransformBackToHuman();
    }
}