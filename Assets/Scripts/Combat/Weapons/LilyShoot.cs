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

    [Header("Visual Setup (In-Game)")]
    [Tooltip("The SpriteRenderer on the player that holds the gun")]
    public SpriteRenderer weaponRenderer; 

    [Header("UI Setup")]
    public Image ammoBar;
    [Tooltip("The UI Image on your Canvas that shows the current weapon icon")]
    public Image weaponIconDisplay; 

    [Header("Art Assets")]
    [Tooltip("The sprite the player actually holds")]
    public Sprite weaponWorldSprite;
    [Tooltip("The icon that appears in the UI")]
    public Sprite weaponUIIcon;

    [Header("Settings")]
    public float flowerSpeed = 18f;
    public Color teamColor = Color.cyan;
    public float fireRate = 0.12f;
    public float maxRange = 10f;
    public float rotationOffset = 0f; // Adjusted if your sprite points up instead of right

    [Header("Ammo Stats")]
    public int maxAmmo = 40;
    public float reloadTime = 1.2f;

    private int _currentAmmo;
    private float _nextFireTime;
    private bool _isReloading;

    void Start()
    {
        _currentAmmo = maxAmmo;
        if (cam == null) cam = Camera.main;
        
        // Auto-find weapon renderer if empty
        if (weaponRenderer == null) weaponRenderer = GetComponent<SpriteRenderer>();

        // 1. Setup the UI Icon
        if (weaponIconDisplay != null && weaponUIIcon != null)
        {
            weaponIconDisplay.sprite = weaponUIIcon;
            weaponIconDisplay.preserveAspect = true;
            weaponIconDisplay.enabled = true;
        }

        // 2. Setup the In-Game Sprite
        if (weaponRenderer != null && weaponWorldSprite != null)
        {
            weaponRenderer.sprite = weaponWorldSprite;
        }

        UpdateAmmoUI();
    }

    void Update()
    {
        RotateTowardsMouse();

        if (_isReloading) return;

        // Press R to reload manually
        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // Auto reload if empty
        if (_currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + fireRate;
        }
    }

    void RotateTowardsMouse()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - (Vector2)transform.position;
        
        // Calculate angle
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        
        // Apply rotation with offset
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // --- SPRITE FLIPPING LOGIC ---
        // If aiming to the left (angle > 90 or < -90), flip the sprite Y
        if (weaponRenderer != null)
        {
            if (Mathf.Abs(angle) > 90)
                weaponRenderer.flipY = true;
            else
                weaponRenderer.flipY = false;
        }
    }

    void Shoot()
    {
        _currentAmmo--;
        UpdateAmmoUI();

        // Instantiate projectile
        GameObject projGO = Instantiate(flowerProjectilePrefab, firePoint.position, firePoint.rotation);
        
        // Setup projectile logic
        FlowerCollision projectileLogic = projGO.AddComponent<FlowerCollision>();
        
        // Note: We use firePoint.right because we removed the "-90" form the rotation logic
        // If your bullets shoot sideways, change 'firePoint.right' to 'firePoint.up'
        projectileLogic.Setup(splatPrefab, teamColor, firePoint.right * flowerSpeed, maxRange);
    }

    IEnumerator Reload()
    {
        _isReloading = true;
        if (ammoBar != null) ammoBar.color = new Color(1, 1, 1, 0.5f); // Dim bar
        
        yield return new WaitForSeconds(reloadTime);
        
        _currentAmmo = maxAmmo;
        _isReloading = false;
        
        if (ammoBar != null) ammoBar.color = Color.white; // Restore color
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoBar != null) 
            ammoBar.fillAmount = (float)_currentAmmo / maxAmmo;
    }
}

// --- PROJECTILE & INK LOGIC (Unchanged) ---
public class FlowerCollision : MonoBehaviour
{
    private GameObject _splatPrefab;
    private Color _paintColor;
    private float _dropTimer;
    private float _dropInterval = 0.05f;
    private Vector2 _startPos;
    private float _maxDistance;
    private Rigidbody2D _rb; // Cache RB

    public void Setup(GameObject splat, Color color, Vector2 velocity, float range)
    {
        _splatPrefab = splat;
        _paintColor = color;
        _maxDistance = range;
        _startPos = transform.position;

        if (TryGetComponent(out SpriteRenderer sr)) sr.color = color;

        _rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.linearVelocity = velocity; // Updated for Unity 6 (use .velocity for older versions)

        CircleCollider2D col = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        Destroy(gameObject, 5f);
    }

    void Update()
    {
        _dropTimer += Time.deltaTime;
        if (_dropTimer >= _dropInterval) { SpawnPaint(0.45f); _dropTimer = 0; }

        if (Vector2.Distance(_startPos, transform.position) >= _maxDistance) HandleImpact();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground")) HandleImpact();
    }

    void HandleImpact()
    {
        SpawnPaint(1.3f);
        Destroy(gameObject);
    }

    void SpawnPaint(float scaleMult)
    {
        if (_splatPrefab == null) return;

        GameObject splat = Instantiate(_splatPrefab, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        splat.tag = "Ink";

        CircleCollider2D col = splat.GetComponent<CircleCollider2D>() ?? splat.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        if (splat.TryGetComponent(out SpriteRenderer sr))
        {
            sr.color = _paintColor;
            sr.sortingOrder = -1;
        }
        splat.transform.localScale *= Random.Range(0.8f, 1.2f) * scaleMult;
    }
}