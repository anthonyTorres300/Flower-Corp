using UnityEngine;

public class LilyShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject flowerProjectilePrefab; 
    public GameObject splatPrefab;        
    public Transform firePoint;            

    [Header("Settings")]
    public float flowerSpeed = 15f;
    public Color flowerColor = Color.cyan;
    public float fireRate = 0.15f;
    private float nextFireTime;

    void Update()
    {
        RotateTowardsMouse();

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void RotateTowardsMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - (Vector2)transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(flowerProjectilePrefab, firePoint.position, firePoint.rotation);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = flowerColor;

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb == null) rb = projectile.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.linearVelocity = firePoint.up * flowerSpeed;

        FlowerCollision handler = projectile.AddComponent<FlowerCollision>();
        handler.flowerPrefab = splatPrefab;
        handler.flowerColor = flowerColor;
    }
}

public class FlowerCollision : MonoBehaviour
{
    public GameObject flowerPrefab;
    public Color flowerColor;

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject splat = Instantiate(flowerPrefab, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        splat.GetComponent<SpriteRenderer>().color = flowerColor;

        splat.transform.localScale *= Random.Range(0.8f, 1.5f);

        Destroy(gameObject); 
    }
}