using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // Ensures the object has an AudioSource
public class LilyShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject flowerProjectilePrefab;
    public GameObject splatPrefab;
    public Transform firePoint;
    public Camera cam;

    [Header("Audio Setup")] // --- NEW SECTION ---
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    [Range(0f, 0.2f)] public float pitchVariation = 0.05f;

    [Header("Visual Setup (In-Game)")]
    [Tooltip("The SpriteRenderer on the player that holds the gun")]
    public SpriteRenderer weaponRenderer; 

    [Header("UI Setup")]
    public Image ammoBar;
    [Tooltip("The UI Image on your Canvas that shows the current weapon icon")]
    public Image weaponIconDisplay; 

    [Header("Art Assets")]
    public Sprite weaponWorldSprite;
    public Sprite weaponUIIcon;

    [Header("Settings")]
    public float flowerSpeed = 18f;
    public Color teamColor = Color.cyan;
    public float fireRate = 0.12f;
    public float maxRange = 10f;
    public float rotationOffset = 0f; 

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
        
        // Auto-find components if empty
        if (weaponRenderer == null) weaponRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (weaponIconDisplay != null && weaponUIIcon != null)
        {
            weaponIconDisplay.sprite = weaponUIIcon;
            weaponIconDisplay.preserveAspect = true;
        }

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

        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

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
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        if (weaponRenderer != null)
        {
            weaponRenderer.flipY = Mathf.Abs(angle) > 90;
        }
    }

    void Shoot()
    {
        _currentAmmo--;
        UpdateAmmoUI();

        // --- PLAY SHOOT SOUND ---
        if (audioSource != null && shootSound != null)
        {
            // Slight pitch shift makes rapid fire sound better
            audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
            audioSource.PlayOneShot(shootSound);
        }

        GameObject projGO = Instantiate(flowerProjectilePrefab, firePoint.position, firePoint.rotation);
        FlowerCollision projectileLogic = projGO.AddComponent<FlowerCollision>();
        projectileLogic.Setup(splatPrefab, teamColor, firePoint.right * flowerSpeed, maxRange);
    }

    IEnumerator Reload()
    {
        _isReloading = true;

        // --- PLAY RELOAD SOUND ---
        if (audioSource != null && reloadSound != null)
        {
            audioSource.pitch = 1f; // Reset pitch for reload
            audioSource.PlayOneShot(reloadSound);
        }

        if (ammoBar != null) ammoBar.color = new Color(1, 1, 1, 0.5f);
        
        yield return new WaitForSeconds(reloadTime);
        
        _currentAmmo = maxAmmo;
        _isReloading = false;
        
        if (ammoBar != null) ammoBar.color = Color.white;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoBar != null) 
            ammoBar.fillAmount = (float)_currentAmmo / maxAmmo;
    }
}
public class FlowerCollision : MonoBehaviour
{
    private GameObject _splatPrefab;
    private Color _paintColor;
    private float _dropTimer;
    private float _dropInterval = 0.05f;
    private Vector2 _startPos;
    private float _maxDistance;
    private Rigidbody2D _rb; 

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
        
        // Use .velocity if you are on an older version of Unity
        _rb.linearVelocity = velocity; 

        CircleCollider2D col = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        Destroy(gameObject, 5f);
    }

    void Update()
    {
        _dropTimer += Time.deltaTime;
        if (_dropTimer >= _dropInterval) 
        { 
            SpawnPaint(0.45f); 
            _dropTimer = 0; 
        }

        if (Vector2.Distance(_startPos, transform.position) >= _maxDistance) 
            HandleImpact();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground")) 
            HandleImpact();
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