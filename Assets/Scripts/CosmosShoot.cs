using UnityEngine;
using System.Collections;

public class CosmosWeapon : MonoBehaviour
{
    public enum WeaponType { Pistol, MachineGun, Shotgun, Sniper, BurstRifle }

    [Header("Setup")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Camera cam;

    [Header("Weapon Configuration")]
    public WeaponType currentWeapon;

    // This hidden variable helps us detect changes in the Inspector
    [HideInInspector]
    public WeaponType lastWeaponType;

    [Header("Stats (Auto-Updates on Change)")]
    public float bulletSpeed = 20f;
    public float fireRate = 0.5f;
    public float spread = 0f;
    public int projectileCount = 1;

    private float nextFireTime = 0f;

    // ---------------------------------------------------------
    // EDITOR MAGIC: This runs automatically when you change values in the Inspector
    // ---------------------------------------------------------
    void OnValidate()
    {
        // If the weapon type changed, update the stats immediately
        if (currentWeapon != lastWeaponType)
        {
            EquipWeapon(currentWeapon);
            lastWeaponType = currentWeapon;
        }
    }

    void Start()
    {
        // Ensure stats are set correctly when the game starts
        EquipWeapon(currentWeapon);
    }

    void Update()
    {
        HandleAiming();
        HandleShooting();
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

        // Machine Gun allows holding the button; others require clicking
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

            switch (currentWeapon)
            {
                case WeaponType.BurstRifle:
                    StartCoroutine(FireBurst());
                    break;
                case WeaponType.Shotgun:
                    FireSpread(projectileCount, spread);
                    break;
                default:
                    FireSpread(1, spread); // Standard fire
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
            {
                // Random spray for rapid fire/pistols
                currentSpread = Random.Range(-spreadAngle, spreadAngle);
            }
            else
            {
                // Fixed pattern for Shotguns
                currentSpread = -centeringOffset + (angleStep * i);
            }

            Quaternion rotationMod = Quaternion.Euler(0, 0, currentSpread);
            SpawnBullet(rotationMod);
        }
    }

    IEnumerator FireBurst()
    {
        // Fire 3 shots with a small delay between them
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

    // This function sets the hardcoded values based on type
    public void EquipWeapon(WeaponType type)
    {
        currentWeapon = type;
        switch (type)
        {
            case WeaponType.Pistol:
                fireRate = 0.4f;
                spread = 2f;
                projectileCount = 1;
                bulletSpeed = 20f;
                break;
            case WeaponType.MachineGun:
                fireRate = 0.1f;
                spread = 12f;
                projectileCount = 1;
                bulletSpeed = 22f;
                break;
            case WeaponType.Shotgun:
                fireRate = 0.8f;
                spread = 35f;
                projectileCount = 5;
                bulletSpeed = 18f;
                break;
            case WeaponType.Sniper:
                fireRate = 1.5f;
                spread = 0f;
                projectileCount = 1;
                bulletSpeed = 45f;
                break;
            case WeaponType.BurstRifle:
                fireRate = 0.6f;
                spread = 1f;
                projectileCount = 1;
                bulletSpeed = 25f;
                break;
        }
    }
}