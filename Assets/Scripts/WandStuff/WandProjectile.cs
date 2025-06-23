using UnityEngine;
using TMPro;

public class WandProjectile : MonoBehaviour
{
    [Header("Magic Spell Settings")]
    //Magic Spell Reference
    public GameObject zap;

    //Wand forces
    public float shootForce = 30f;
    public float upwardForce;

    //Stats for wand
    public float timeBetweenShooting = 0.25f;
    public float spread;
    public float reloadTime = 1f;
    public float timeBetweenShots;

    public int zapStorage = 3;
    public int bulletPerTap = 1;

    public bool allowButtonHold = false;

    [Header("Equipment Check")]
    public InteractableWand wandScript;

    private int zapsLeft, zapsShot;

    private bool shooting, readyToShoot, reloading, allowInvoke;

    public new Camera camera;
    public Transform attackPoint;

    //Graphics
    public TextMeshProUGUI zapDisplay;

    private void Awake()
    {
        //Make sure zaps are full
        zapsLeft = zapStorage;
        readyToShoot = true;
        allowInvoke = true;
        
        // Try to find the wand script if not assigned
        if (wandScript == null)
        {
            wandScript = GetComponent<InteractableWand>();
        }
        
        // Try to find camera if not assigned
        if (camera == null)
        {
            camera = Camera.main;
        }
    }

    private void Update()
    {
        // Only allow input if wand is equipped
        if (IsWandEquipped())
        {
            MyInput();
        }

        //Set Magic display
        if (zapDisplay != null)
        {
            zapDisplay.SetText(zapsLeft / bulletPerTap + " / " + zapStorage / bulletPerTap);
        }
    }

    private bool IsWandEquipped()
    {
        // Check if wand is picked up (equipped)
        return wandScript != null && wandScript.IsPickedUp();
    }

    private void MyInput()
    {
        // Only allow single clicks, not holding
        shooting = Input.GetKeyDown(KeyCode.Mouse0);

        // Prevent shooting when out of ammo
        if (shooting && zapsLeft <= 0)
        {
            Debug.Log("Out of magic! Complete objectives to recharge wand.");
            return;
        }

        //Casting spells - only if we have ammo
        if (readyToShoot && shooting && !reloading && zapsLeft > 0)
        {
            zapsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        //Find the exact hit position using raycast
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;

        //Check if the ray hit something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(75);
        }

        //Calculate the direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        //Instantiate magic projectile
        GameObject currentSpell = Instantiate(zap, attackPoint.position, Quaternion.identity);
        
        //Rotate spell to shoot direction
        currentSpell.transform.forward = directionWithoutSpread.normalized;
        
        //Make sure the spell has a Rigidbody and configure it
        Rigidbody spellRb = currentSpell.GetComponent<Rigidbody>();
        if (spellRb == null)
        {
            spellRb = currentSpell.AddComponent<Rigidbody>();
        }
        
        // Configure rigidbody settings
        spellRb.isKinematic = false;
        spellRb.useGravity = false;
        spellRb.linearDamping = 0f;
        spellRb.angularDamping = 0f;
        spellRb.mass = 1f;
        
        //Add force to the spell
        spellRb.AddForce(directionWithoutSpread.normalized * shootForce, ForceMode.Impulse);

        zapsLeft--;
        zapsShot++;

        //Invoke resetShot function to delay next spell casting
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Debug.Log("Recharging wand...");
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        zapsLeft = zapStorage;
        reloading = false;
        Debug.Log("Wand recharged!");
    }

    // Public method to recharge wand from objectives
    public void RechargeWand()
    {
        zapsLeft = zapStorage;
        Debug.Log("Wand recharged by completing objective!");
    }

    // Public method to check if wand needs recharging
    public bool NeedsRecharge()
    {
        return zapsLeft <= 0;
    }

    // Public method to get current ammo count
    public int GetCurrentAmmo()
    {
        return zapsLeft;
    }
}