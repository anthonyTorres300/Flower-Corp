using UnityEngine;
using UnityEngine.UI; // Required for UI
using System.Collections;

public class LilyShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject flowerProjectilePrefab;
    public GameObject splatPrefab;
    public Transform firePoint;
    public Camera cam;

    [Header("UI Setup")]
    public Image ammoBar; // Drag your Filled Image here

    [Header("Settings")]
    public float flowerSpeed = 15f;
    public Color flowerColor = Color.cyan;
    public float fireRate = 0.15f;
    private float nextFireTime;

    [Header("Ammo Stats")]
    public int maxAmmo = 20;
    public float reloadTime = 1.5f;
    private int currentAmmo;
    private bool isReloading = false;

    void Start()
    {
        currentAmmo = maxAmmo;
        if (cam == null) cam = Camera.main;
        UpdateAmmoUI();
    }

    void Update()
    {
        RotateTowardsMouse();
        UpdateAmmoUI();

        if (isReloading) return;

        // Auto-reload if empty
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // Shoot input
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void RotateTowardsMouse()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - (Vector2)transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        currentAmmo--; // Reduce Ammo

        GameObject projectile = Instantiate(flowerProjectilePrefab, firePoint.position, firePoint.rotation);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = flowerColor;

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb == null) rb = projectile.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;

        // Note: linearVelocity is for Unity 6+. Use rb.velocity for older versions.
        rb.linearVelocity = firePoint.up * flowerSpeed;

        // This is the part that needs the class below!
        FlowerCollision handler = projectile.AddComponent<FlowerCollision>();
        handler.flowerPrefab = splatPrefab;
        handler.flowerColor = flowerColor;
    }

    IEnumerator Reload()
    {
        isReloading = true;
        // Optional: Debug.Log("Reloading...");

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

// ---------------------------------------------------------
// THIS CLASS WAS MISSING BEFORE - IT MUST BE IN THE FILE
// ---------------------------------------------------------
public class FlowerCollision : MonoBehaviour
{
    public GameObject flowerPrefab;
    public Color flowerColor;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
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

            Destroy(gameObject);
        }
    }
}