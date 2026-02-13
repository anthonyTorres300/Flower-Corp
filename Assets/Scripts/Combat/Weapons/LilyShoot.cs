using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LilyShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject flowerProjectilePrefab;
    public GameObject splatPrefab;
    public Transform firePoint;
    public Camera cam;

    [Header("UI")]
    public Image ammoBar;

    [Header("Settings")]
    public float flowerSpeed = 15f;
    public Color flowerColor = Color.cyan;
    public float fireRate = 0.15f;
    public int damage = 5; // added damage field for cupid bonuses

    [Header("Ammo")]
    public int maxAmmo = 20;
    public float reloadTime = 1.5f;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime;
    private SwitchCharacters switchScript;

    void Start()
    {
        currentAmmo = maxAmmo;
        if (cam == null) cam = Camera.main;
        switchScript = GetComponent<SwitchCharacters>();
        UpdateAmmoUI();
    }

    void Update()
    {
        // check if active
        if (switchScript != null && !switchScript.isActive) return;

        UpdateAmmoUI();

        if (isReloading) return;

        // auto reload
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // hold to shoot
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        currentAmmo--;

        GameObject projectile = Instantiate(flowerProjectilePrefab, firePoint.position, firePoint.rotation);

        // color
        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = flowerColor;

        // movement
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb == null) rb = projectile.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = firePoint.up * flowerSpeed;

        // collision handler
        FlowerCollision handler = projectile.AddComponent<FlowerCollision>();
        handler.flowerPrefab = splatPrefab;
        handler.flowerColor = flowerColor;
        handler.damage = damage;

        // tag as flower for cupid conversion
        projectile.tag = "Flower";

        // auto destroy
        Destroy(projectile, 3f);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoBar != null)
        {
            float fillAmount = (float)currentAmmo / maxAmmo;
            ammoBar.fillAmount = fillAmount;
        }
    }
}

public class FlowerCollision : MonoBehaviour
{
    public GameObject flowerPrefab;
    public Color flowerColor;
    public int damage = 5;

    void OnTriggerEnter2D(Collider2D other)
    {
        // hit enemy
        if (other.CompareTag("Enemy"))
        {
            Damageable enemy = other.GetComponent<Damageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // hit wall
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            // spawn splat
            if (flowerPrefab != null)
            {
                GameObject splat = Instantiate(
                    flowerPrefab,
                    transform.position,
                    Quaternion.Euler(0, 0, Random.Range(0, 360))
                );

                var sr = splat.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = flowerColor;

                splat.transform.localScale *= Random.Range(0.6f, 1.3f);
            }
            Destroy(gameObject);
        }
    }
}