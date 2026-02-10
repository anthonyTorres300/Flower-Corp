using UnityEngine;

public class CosmosShoot : MonoBehaviour
{
    [Header("Setup")]
    public Transform firePoint;       
    public GameObject bulletPrefab;   
    public Camera cam;                

    [Header("Stats")]
    public float bulletSpeed = 20f;

    void Update()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 lookDir = mousePos - (Vector2)transform.position;

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Input.GetButtonDown("Fire1")) Shoot();
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.AddForce(firePoint.right * bulletSpeed, ForceMode2D.Impulse);

        Destroy(bullet, 2.0f);
    }
}
