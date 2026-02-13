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

    [Header("UI Setup")]
    public Image ammoBar;

    [Header("Settings")]
    public float flowerSpeed = 18f;
    public Color teamColor = Color.cyan;
    public float fireRate = 0.12f;
    public float maxRange = 10f;

    [Header("Ammo Stats")]
    public int maxAmmo = 40;
    public float reloadTime = 1.2f;

    private int _currentAmmo;
    private float _nextFireTime;
    private bool _isReloading;
    private SwitchCharacters switchScript;

    void Start()
    {
        _currentAmmo = maxAmmo;
        if (cam == null) cam = Camera.main;
        switchScript = GetComponent<SwitchCharacters>();
        UpdateAmmoUI();
    }

    void Update()
    {
        RotateTowardsMouse();

        if (_isReloading) return;

        if (Input.GetKeyDown(KeyCode.R) || _currentAmmo <= 0)
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

    void Shoot()
    {
        _currentAmmo--;
        UpdateAmmoUI();

        GameObject projGO = Instantiate(flowerProjectilePrefab, firePoint.position, firePoint.rotation);
        FlowerCollision projectileLogic = projGO.AddComponent<FlowerCollision>();
        projectileLogic.Setup(splatPrefab, teamColor, firePoint.up * flowerSpeed, maxRange);
    }

    IEnumerator Reload()
    {
        _isReloading = true;
        if (ammoBar != null) ammoBar.color = new Color(1, 1, 1, 0.5f);
        yield return new WaitForSeconds(reloadTime);
        _currentAmmo = maxAmmo;
        _isReloading = false;
        if (ammoBar != null) ammoBar.color = Color.white;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoBar != null) ammoBar.fillAmount = (float)_currentAmmo / maxAmmo;
    }

    void RotateTowardsMouse()
    {
        if (cam == null) return;

        // Get mouse position in world space
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = -cam.transform.position.z;

        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);

        // Calculate direction to mouse
        Vector3 lookDir = mouseWorldPosition - transform.position;

        // Calculate angle
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // Apply rotation
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}

// --- PROJECTILE & INK LOGIC ---
public class FlowerCollision : MonoBehaviour
{
    private GameObject _splatPrefab;
    private Color _paintColor;
    private float _dropTimer;
    private float _dropInterval = 0.05f;
    private Vector2 _startPos;
    private float _maxDistance;

    public void Setup(GameObject splat, Color color, Vector2 velocity, float range)
    {
        _splatPrefab = splat;
        _paintColor = color;
        _maxDistance = range;
        _startPos = transform.position;

        if (TryGetComponent(out SpriteRenderer sr)) sr.color = color;

        Rigidbody2D rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = velocity;

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

        // --- REGISTRATION SETUP ---
        // Give the splat a Tag so the player can find it
        splat.tag = "Ink";

        // Give it a trigger collider so the player can "step" into it
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