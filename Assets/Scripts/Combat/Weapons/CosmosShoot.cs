using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // Ensures the object has an AudioSource
public class CosmosWeapon : MonoBehaviour
{
    public enum WeaponType { Pistol, MachineGun, Shotgun, Sniper }

    [Header("Setup")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Camera cam;

    [Header("Visual Setup (In-Game)")]
    [Tooltip("The SpriteRenderer on the player that holds the gun")]
    public SpriteRenderer weaponRenderer;

    [Header("UI Setup")]
    [Tooltip("The UI Image on your Canvas that shows the current weapon icon")]
    public Image weaponIconDisplay;
    public Image ammoBar;

    [Header("Audio Setup")] // --- NEW SECTION ---
    public AudioSource audioSource;
    [Range(0f, 0.2f)] public float pitchVariation = 0.05f; // Makes guns sound less robotic
    
    [Space(5)]
    public AudioClip pistolSFX;
    public AudioClip machineGunSFX;
    public AudioClip shotgunSFX;
    public AudioClip sniperSFX;

    [Header("Art: In-Game Sprites")]
    public Sprite pistolWorldSprite;
    public Sprite machineGunWorldSprite;
    public Sprite shotgunWorldSprite;
    public Sprite sniperWorldSprite;

    [Header("Art: UI Icons")]
    public Sprite pistolUIIcon;
    public Sprite machineGunUIIcon;
    public Sprite shotgunUIIcon;
    public Sprite sniperUIIcon;

    [Header("Weapon Configuration")]
    public WeaponType currentWeapon;
    [HideInInspector] public WeaponType lastWeaponType;
    [HideInInspector] public bool isInputLocked = false;

    // Stats (Auto-Updated)
    [HideInInspector] public float bulletSpeed;
    [HideInInspector] public float fireRate;
    [HideInInspector] public float spread;
    [HideInInspector] public int projectileCount;
    [HideInInspector] public int maxAmmo;
    [HideInInspector] public float reloadTime;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    // --- NEW VARIABLE: Holds the sound for the currently equipped gun ---
    private AudioClip currentShootClip; 

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
        if (cam == null) cam = Camera.main;
        if (weaponRenderer == null) weaponRenderer = GetComponent<SpriteRenderer>();
        
        // Grab AudioSource if not manually assigned
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        EquipWeapon(currentWeapon);
    }

    void Update()
    {
        if (isInputLocked) return;

        UpdateAmmoUI();

        if (isReloading) return;

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
            float fillAmount = (float)currentAmmo / maxAmmo;
            ammoBar.fillAmount = fillAmount;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        // Optional: You could add a Reload Sound here in the future
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void HandleAiming()
    {
        if (cam == null) return;

        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = -cam.transform.position.z;

        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);
        Vector3 lookDir = mouseWorldPosition - transform.position;

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        if (weaponRenderer != null)
        {
            weaponRenderer.flipY = Mathf.Abs(angle) > 90;
        }
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
            currentAmmo--;
            
            // --- PLAY SOUND ---
            PlayShootSound();

            if (currentWeapon == WeaponType.Shotgun)
            {
                FireSpread(projectileCount, spread);
            }
            else
            {
                FireSpread(1, spread);
            }
        }
    }

    // --- NEW HELPER METHOD FOR AUDIO ---
    void PlayShootSound()
    {
        if (audioSource != null && currentShootClip != null)
        {
            // Randomize pitch slightly for realism
            audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
            
            // PlayOneShot allows sounds to overlap (good for machine guns)
            audioSource.PlayOneShot(currentShootClip);
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

            Quaternion spreadRotation = Quaternion.Euler(0, 0, currentSpread);
            SpawnBullet(spreadRotation);
        }
    }

    void SpawnBullet(Quaternion spreadRotation)
    {
        Quaternion finalRotation = firePoint.rotation * spreadRotation;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, finalRotation);

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
        
        Sprite worldSprite = null;
        Sprite uiIcon = null;

        // 1. SET STATS, SPRITES, AND AUDIO
        switch (type)
        {
            case WeaponType.Pistol:
                fireRate = 0.4f; spread = 2f; projectileCount = 1; bulletSpeed = 20f; maxAmmo = 12; reloadTime = 1f;
                worldSprite = pistolWorldSprite;
                uiIcon = pistolUIIcon;
                currentShootClip = pistolSFX; // Assign Pistol Audio
                break;

            case WeaponType.MachineGun:
                fireRate = 0.1f; spread = 12f; projectileCount = 1; bulletSpeed = 22f; maxAmmo = 30; reloadTime = 2.5f;
                worldSprite = machineGunWorldSprite;
                uiIcon = machineGunUIIcon;
                currentShootClip = machineGunSFX; // Assign MG Audio
                break;

            case WeaponType.Shotgun:
                fireRate = 0.8f; spread = 35f; projectileCount = 5; bulletSpeed = 18f; maxAmmo = 5; reloadTime = 1.5f;
                worldSprite = shotgunWorldSprite;
                uiIcon = shotgunUIIcon;
                currentShootClip = shotgunSFX; // Assign Shotgun Audio
                break;

            case WeaponType.Sniper:
                fireRate = 1.5f; spread = 0f; projectileCount = 1; bulletSpeed = 45f; maxAmmo = 5; reloadTime = 3.0f;
                worldSprite = sniperWorldSprite;
                uiIcon = sniperUIIcon;
                currentShootClip = sniperSFX; // Assign Sniper Audio
                break;
        }

        // 2. UPDATE WORLD SPRITE (In Hand)
        if (weaponRenderer != null && worldSprite != null)
        {
            weaponRenderer.sprite = worldSprite;
        }

        // 3. UPDATE UI ICON (On Screen)
        if (weaponIconDisplay != null && uiIcon != null)
        {
            weaponIconDisplay.sprite = uiIcon;
            weaponIconDisplay.preserveAspect = true; 
        }

        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > maxAmmo) currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }
}