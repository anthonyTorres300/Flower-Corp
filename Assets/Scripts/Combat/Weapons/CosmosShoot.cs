using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CosmosShoot : MonoBehaviour
{
    [Header("Setup")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Camera cam;

    [Header("UI")]
    public Image ammoBar;

    [Header("Weapon Stats")]
    public float bulletSpeed = 20f;
    public float fireRate = 0.5f;
    public int damage = 20;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public float reloadTime = 1.5f;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;
    private SwitchCharacters switchScript;

    void Start()
    {
        currentAmmo = maxAmmo;
        if (cam == null) cam = Camera.main;
        switchScript = GetComponent<SwitchCharacters>();
    }

    void Update()
    {
        // check if this character is active
        if (switchScript != null && !switchScript.isActive) return;

        UpdateAmmoUI();

        if (isReloading) return;

        // auto reload when empty
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
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void HandleShooting()
    {
        if (Time.time < nextFireTime) return;

        if (Input.GetButtonDown("Fire1"))
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;
            FireBullet();
        }
    }

    void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // make bullet move in the direction the fire point is facing
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            // fire in the UP direction of the fire point (local space)
            rb.linearVelocity = firePoint.up * bulletSpeed;
        }

        // add damage component
        Bulletdamage dmg = bullet.GetComponent<Bulletdamage>();
        if (dmg == null) dmg = bullet.AddComponent<Bulletdamage>();
        dmg.damage = damage;

        // destroy after 3 seconds
        Destroy(bullet, 3f);
    }
}