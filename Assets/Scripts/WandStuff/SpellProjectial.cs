using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifeTime = 5f;
    public float damage = 1f;
    
    [Header("Frog Transformation")]
    public bool canTransformToFrog = true;
    
    [Header("Effects")]
    public GameObject hitEffect; // Particle effect when hitting something
    public AudioClip hitSound; // Sound when hitting
    
    private bool hasHit = false;

    private void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent multiple hits
        if (hasHit) return;
        
        // Don't hit the caster
        if (other.transform.root == transform.root) return;
        
        Debug.Log($"Spell hit: {other.gameObject.name}");
        
        // Check if we hit a player - look on the hit object AND its parent
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        
        // If not found on the hit object, check the parent
        if (playerHealth == null && other.transform.parent != null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }
        
        // Also try checking by tag
        if (playerHealth == null && (other.CompareTag("Human") || other.CompareTag("Player")))
        {
            // Look for PlayerHealth in parent hierarchy
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }
        
        if (playerHealth != null)
        {
            Debug.Log($"Found PlayerHealth on: {playerHealth.gameObject.name}");
            HandlePlayerHit(playerHealth, other);
        }
        else
        {
            Debug.Log($"No PlayerHealth found on {other.gameObject.name} or its parents");
            // Hit something else (wall, object, etc.)
            HandleEnvironmentHit(other);
        }
    }
    
    private void HandlePlayerHit(PlayerHealth playerHealth, Collider hitCollider)
    {
        hasHit = true;
        
        // Only transform humans to frogs
        if (playerHealth.IsHuman() && canTransformToFrog)
        {
            playerHealth.TransformToFrog();
            Debug.Log($"Transformed {hitCollider.gameObject.name} into a frog!");
        }
        else if (playerHealth.IsFrog())
        {
            Debug.Log($"{hitCollider.gameObject.name} is already a frog!");
        }
        
        // Create hit effects
        CreateHitEffects(hitCollider.transform.position);
        
        // Destroy the projectile
        DestroyProjectile();
    }
    
    private void HandleEnvironmentHit(Collider hitCollider)
    {
        hasHit = true;
        
        Debug.Log($"Spell hit environment: {hitCollider.gameObject.name}");
        
        // Create hit effects
        CreateHitEffects(hitCollider.transform.position);
        
        // Destroy the projectile
        DestroyProjectile();
    }
    
    private void CreateHitEffects(Vector3 position)
    {
        // Spawn hit effect
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, Quaternion.identity);
            // Destroy effect after a few seconds
            Destroy(effect, 3f);
        }
        
        // Play hit sound
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }
    
    private void DestroyProjectile()
    {
        // Add a small delay to ensure effects are created
        Destroy(gameObject, 0.1f);
    }
    
    // Optional: Add a method to set projectile properties from the wand
    public void Initialize(float projectileLifeTime, bool enableFrogTransform)
    {
        lifeTime = projectileLifeTime;
        canTransformToFrog = enableFrogTransform;
    }
}