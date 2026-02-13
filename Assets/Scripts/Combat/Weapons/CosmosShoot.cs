using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum WeaponType
{
    Pistol,
    MachineGun,
    Shotgun,
    Sniper,
    BurstRifle
}

public class CosmosShoot : MonoBehaviour
{
    [Header("Setup")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Camera cam;

    [Header("UI Setup")]
    public Image ammoBar;

    [Header("Weapon Configuration")]
    public WeaponType currentWeapon;
    [HideInInspector] public WeaponType lastWeaponType;

    // --- NEW VARIABLE ---
    // This allows the WeaponSelector script to stop us from shooting while in the menu
    [HideInInspector] public bool isInputLocked = false;

    [Header("Stats (Auto-Updates)")]
    public float bulletSpeed = 20f;
    public float fireRate = 0.5f;
    public int damage = 20;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public float reloadTime = 1.5f;

    [Header("Spread Settings")]
    public int projectileCount = 1;
    public float spread = 5f;
    public float spreadAngle = 10f;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    [Header("Adjustments")]
    // IF YOUR GUN POINTS WRONG: Change this to -90, 90, or 180
    public float rotationOffset = 0f;

    void OnValidate()
    {
        if (currentWeapon != lastWeaponType)
        {
            EquipWeapon(currentWeapon);
            lastWeaponType = currentWeapon;
        }
    }

    void Start()
    {
        if (cam == null) cam = Camera.main; // Auto-find camera
        EquipWeapon(currentWeapon);
    }

    void Update()
    {
        // --- NEW CHECK ---
        // If input is locked (menu is open), do nothing.
        if (isInputLocked) return;

        UpdateAmmoUI();

        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        HandleShooting();
    }

    void UpdateAmmoUI()
    {
        if (ammoBar != null)
        {
            float fillAmount = (float)currentAmmo / maxAmmo;
            ammoBar.fillAmount = fillAmount;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        // Debug.Log("Reloading..."); 
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        // Debug.Log("Reload Complete!");
    }

    void HandleAiming()
    {
        if (cam == null) return;

        // 1. Get Mouse Position correctly
        // We set z to -cam.transform.position.z to ensure distance is calculated from the camera plane
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = -cam.transform.position.z;

        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);

        // 2. Calculate Direction
        Vector3 lookDir = mouseWorldPosition - transform.position;

        // 3. Calculate Angle
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // 4. Apply Rotation (Adding the offset here)
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // DEBUG: Draw a line in the Scene view to show where the math thinks we are aiming
        Debug.DrawLine(transform.position, mouseWorldPosition, Color.red);
    }

    void HandleShooting()
    {
        if (Time.time < nextFireTime) return;

        if (Input.GetButtonDown("Fire1"))
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;

            switch (currentWeapon)
            {
                case WeaponType.BurstRifle:
                    StartCoroutine(FireBurst());
                    break;
                case WeaponType.Shotgun:
                    FireSpread(projectileCount, spread);
                    break;
                default:
                    FireSpread(1, spread);
                    break;
            }
        }
    }

    void FireSpread(int count, float spreadAngle)
    {
        float angleStep = count > 1 ? spreadAngle / (count - 1) : 0;
        float centeringOffset = count > 1 ? spreadAngle / 2 : 0;

        for (int i = 0; i < count; i++)
        {
            float currentSpread = 0f;

            if (currentWeapon == WeaponType.MachineGun || currentWeapon == WeaponType.Pistol)
                currentSpread = Random.Range(-spreadAngle, spreadAngle);
            else
                currentSpread = -centeringOffset + (angleStep * i);

            // We pass the spread rotation to the bullet spawner
            Quaternion spreadRotation = Quaternion.Euler(0, 0, currentSpread);
            SpawnBullet(spreadRotation);
        }
    }

    IEnumerator FireBurst()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnBullet(Quaternion.identity);
            yield return new WaitForSeconds(0.08f);
        }
    }

    void SpawnBullet(Quaternion spreadRotation)
    {
        // 1. Calculate the final rotation: FirePoint rotation + Spread
        Quaternion finalRotation = firePoint.rotation * spreadRotation;

        // 2. Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, finalRotation);

        // make bullet move in the direction the fire point is facing
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // IMPORTANT: We use the bullet's own "Right" vector (Red Arrow) 
            // because we just rotated the bullet to face the correct way.
            rb.AddForce(bullet.transform.right * bulletSpeed, ForceMode2D.Impulse);
        }

        // add damage component
        Bulletdamage dmg = bullet.GetComponent<Bulletdamage>();
        if (dmg == null) dmg = bullet.AddComponent<Bulletdamage>();
        dmg.damage = damage;
    }

    public void EquipWeapon(WeaponType type)
    {
        currentWeapon = type;
        currentAmmo = maxAmmo;

        switch (type)
        {
            case WeaponType.Pistol:
                fireRate = 0.4f; spread = 2f; projectileCount = 1; bulletSpeed = 20f; maxAmmo = 12; reloadTime = 1f; break;
            case WeaponType.MachineGun:
                fireRate = 0.1f; spread = 12f; projectileCount = 1; bulletSpeed = 22f; maxAmmo = 30; reloadTime = 2.5f; break;
            case WeaponType.Shotgun:
                fireRate = 0.8f; spread = 35f; projectileCount = 5; bulletSpeed = 18f; maxAmmo = 5; reloadTime = 1.5f; break;
            case WeaponType.Sniper:
                fireRate = 1.5f; spread = 0f; projectileCount = 1; bulletSpeed = 45f; maxAmmo = 5; reloadTime = 3.0f; break;
            case WeaponType.BurstRifle:
                fireRate = 0.6f; spread = 1f; projectileCount = 1; bulletSpeed = 25f; maxAmmo = 15; reloadTime = 2.0f; break;
        }
        currentAmmo = maxAmmo;
    }

    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > maxAmmo) currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }
}
