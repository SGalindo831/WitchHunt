using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public float lifeTime = 5f; // How long before it disappears
    
    private void Start()
    {
        // Just destroy after a few seconds
        Destroy(gameObject, lifeTime);
    }
}