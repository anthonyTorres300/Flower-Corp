using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required for UI work

public class CosmosWeapon : MonoBehaviour
{
    public enum WeaponType { Pistol, MachineGun, Shotgun, Sniper, BurstRifle }

    [Header("Setup")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Camera cam;

    [Header("UI Setup")]
    public Image ammoBar; // Drag your UI Image here

    [Header("Weapon Configuration")]
    public WeaponType currentWeapon;

    [HideInInspector]
    public WeaponType lastWeaponType;

    [Header("Stats (Auto-Updates on Change)")]
    public float bulletSpeed = 20f;
    public float fireRate = 0.5f;
    public float spread = 0f;
    public int projectileCount = 1;

    [Header("Ammo Stats")]
    public int maxAmmo = 10;
    public float reloadTime = 1.5f;
    private int currentAmmo;
    private bool isReloading = false;

    private float nextFireTime = 0f;

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
        EquipWeapon(currentWeapon);
    }

    void Update()
    {
        // 1. Update UI every frame to match current ammo
        UpdateAmmoUI();

        // 2. Stop player from doing anything if reloading
        if (isReloading) return;

        // 3. Check for manual reload (Optional, e.g., pressing R)
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        HandleAiming();
        HandleShooting();
    }

    void UpdateAmmoUI()
    {
        if (ammoBar != null)
        {
            // Calculate percentage (0.0 to 1.0)
            float fillAmount = (float)currentAmmo / maxAmmo;
            ammoBar.fillAmount = fillAmount;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        // Optional: Play a reload sound here

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reload Complete!");
    }

    void HandleAiming()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - (Vector2)transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void HandleShooting()
    {
        if (Time.time < nextFireTime) return;

        bool isFiring = false;

        if (currentWeapon == WeaponType.MachineGun)
        {
            if (Input.GetButton("Fire1")) isFiring = true;
        }
        else
        {
            if (Input.GetButtonDown("Fire1")) isFiring = true;
        }

        if (isFiring)
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--; // Consume 1 ammo per trigger pull

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

            Quaternion rotationMod = Quaternion.Euler(0, 0, currentSpread);
            SpawnBullet(rotationMod);
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

    void SpawnBullet(Quaternion rotationOffset)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * rotationOffset);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(bullet.transform.right * bulletSpeed, ForceMode2D.Impulse);
        }

        Destroy(bullet, 2.0f);
    }

    public void EquipWeapon(WeaponType type)
    {
        currentWeapon = type;

        // Default Stats
        switch (type)
        {
            case WeaponType.Pistol:
                fireRate = 0.4f; spread = 2f; projectileCount = 1; bulletSpeed = 20f;
                maxAmmo = 12; reloadTime = 1f;
                break;
            case WeaponType.MachineGun:
                fireRate = 0.1f; spread = 12f; projectileCount = 1; bulletSpeed = 22f;
                maxAmmo = 30; reloadTime = 2.5f;
                break;
            case WeaponType.Shotgun:
                fireRate = 0.8f; spread = 35f; projectileCount = 5; bulletSpeed = 18f;
                maxAmmo = 5; reloadTime = 1.5f; // Low ammo count
                break;
            case WeaponType.Sniper:
                fireRate = 1.5f; spread = 0f; projectileCount = 1; bulletSpeed = 45f;
                maxAmmo = 5; reloadTime = 3.0f; // Slow reload
                break;
            case WeaponType.BurstRifle:
                fireRate = 0.6f; spread = 1f; projectileCount = 1; bulletSpeed = 25f;
                maxAmmo = 15; reloadTime = 2.0f;
                break;
        }

        // Refill ammo immediately when switching weapons
        currentAmmo = maxAmmo;
    }

    public void AddAmmo(int amount)
    {
        currentAmmo += amount;

        // Cap ammo at max
        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo;
        }

        UpdateAmmoUI();
        Debug.Log("Ammo Refilled! Current: " + currentAmmo);
    }
}